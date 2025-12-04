using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.Translations;

/// <summary>
/// Specialized query service for Translation entities with authorization pre-filtering
/// </summary>
public class TranslationsQueryService : QueryService<Translation, Guid>
{
    private readonly DataManagerDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public TranslationsQueryService(
        DataManagerDbContext context,
        IFilterHandlerRegistry filterHandlerRegistry,
        IAuthorizationService authorizationService)
        : base(filterHandlerRegistry)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    protected override IQueryable<Translation> DefaultQuery => _context.Translations;

    /// <summary>
    /// Applies authorization filtering to ensure user only sees translations from accessible datasets.
    /// This method can be used for both list queries and single entity queries.
    /// </summary>
    public async Task<IQueryable<Translation>> ApplyAuthorizationAsync(
        IQueryable<Translation> query,
        CancellationToken cancellationToken = default)
    {
        // Get accessible dataset IDs from authorization service
        var (allAccessible, accessibleIds) = await _authorizationService.GetAccessibleDataSetIdsAsync(cancellationToken);

        // If user has access to all datasets, no filtering needed
        if (allAccessible)
        {
            return query;
        }

        if (accessibleIds.Any())
        {
            // User has access to specific datasets - filter by those
            // Note: DataSetId is nullable, so we need to handle that
            query = query.Where(t => t.DataSetId.HasValue && accessibleIds.Contains(t.DataSetId.Value));
        }
        else
        {
            // User has no accessible datasets - return empty query
            // This ensures no data leakage even if authorization returns empty list
            query = query.Where(t => false);
        }

        return query;
    }

    /// <summary>
    /// Prepares a query with authorization pre-filtering, applying filters, includes, and ordering.
    /// Only translations from accessible datasets will be included.
    /// If query is null, uses DefaultQuery from DbContext.
    /// By default, only current versions are returned unless version filtering is explicitly specified.
    /// </summary>
    public override async Task<IQueryable<Translation>> PrepareQueryAsync(
        IQueryable<Translation>? query = null,
        QueryOptions<Translation, Guid>? options = null,
        CancellationToken cancellationToken = default)
    {
        query = GetQuery(query);

        // Apply authorization pre-filter first
        query = await ApplyAuthorizationAsync(query, cancellationToken);

        // Apply default version filter if no version filter is specified in options
        if (options?.Filtering?.QueryFilters != null)
        {
            var hasVersionFilter = options.Filtering.QueryFilters
                .Any(f => f is VersionStatusFilter);
            
            if (!hasVersionFilter)
            {
                // By default, only return current versions
                query = query.Where(t => t.IsCurrentVersion);
            }
        }
        else
        {
            // No filters specified at all, apply default current version filter
            query = query.Where(t => t.IsCurrentVersion);
        }

        // Call base implementation to apply filters, includes, and ordering
        return await base.PrepareQueryAsync(query, options, cancellationToken);
    }

    public class Options : QueryOptions<Translation, Guid, Translation>
    {
        public Options()
        {
            AsNoTracking = true;
        }
    }

    /// <summary>
    /// Fetches translations from a DataSet hierarchy with deduplication based on priority.
    /// This is a CORE method that omits authorization - use with caution!
    /// 
    /// The method:
    /// 1. Traverses the DataSet hierarchy starting from root (breadth-first)
    /// 2. Fetches translations from all datasets in hierarchy
    /// 3. Applies deduplication: translations from higher priority datasets (earlier in hierarchy) 
    ///    take precedence over duplicates from lower priority datasets
    /// 4. Deduplication key: (ResourceName, CultureName, TranslationName)
    /// 
    /// Use case: When a translation exists in the main dataset, any translation with the same
    /// (ResourceName, CultureName, TranslationName) from included datasets is ignored.
    /// </summary>
    /// <param name="rootDataSetId">The root DataSet ID to start hierarchy traversal from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of deduplicated translations respecting hierarchy priority</returns>
    public async Task<List<Translation>> GetTranslationsFromHierarchyAsync(
        Guid rootDataSetId,
        CancellationToken cancellationToken = default)
    {
        // Get the dataset hierarchy IDs in priority order (root first)
        // This is a core method without authorization
        var dataSetIds = await GetDataSetHierarchyIdsWithoutAuthorizationAsync(rootDataSetId, cancellationToken);

        if (!dataSetIds.Any())
        {
            return new List<Translation>();
        }

        // Fetch all translations from all datasets in hierarchy
        // Note: We're NOT applying authorization here as this is a core method
        var allTranslations = await _context.Translations
            .Where(t => t.DataSetId.HasValue && dataSetIds.Contains(t.DataSetId.Value))
            .Where(t => t.IsCurrentVersion) // Only current versions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Group translations by their deduplication key and dataset priority
        var result = new List<Translation>();
        var seenKeys = new HashSet<(string ResourceName, string? CultureName, string TranslationName)>();

        // Process translations in hierarchy order (respecting priority)
        foreach (var dataSetId in dataSetIds)
        {
            var translationsForDataSet = allTranslations
                .Where(t => t.DataSetId == dataSetId)
                .ToList();

            foreach (var translation in translationsForDataSet)
            {
                var key = (translation.ResourceName, translation.CultureName, translation.TranslationName);
                
                // Only add if we haven't seen this key before (higher priority datasets were processed first)
                if (seenKeys.Add(key))
                {
                    result.Add(translation);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Gets all datasets in hierarchical order for a given root dataset ID WITHOUT authorization.
    /// This is a CORE method - use with caution!
    /// Returns a list starting with the root dataset, followed by all included datasets in breadth-first order.
    /// Handles circular references by tracking visited datasets.
    /// </summary>
    /// <param name="rootDataSetId">The ID of the root dataset to start traversal from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of dataset IDs in hierarchical order</returns>
    private async Task<List<Guid>> GetDataSetHierarchyIdsWithoutAuthorizationAsync(
        Guid rootDataSetId,
        CancellationToken cancellationToken = default)
    {
        // Fetch all datasets with their includes from the database WITHOUT authorization
        var allDataSets = await _context.DataSets
            .Include(ds => ds.Includes)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Create a lookup for quick access
        var dataSetLookup = allDataSets.ToDictionary(ds => ds.Id);

        // Check if the root dataset exists
        if (!dataSetLookup.ContainsKey(rootDataSetId))
        {
            return new List<Guid>();
        }

        var result = new List<Guid>();
        var visited = new HashSet<Guid>();
        var queue = new Queue<Guid>();

        // Start with the root dataset
        queue.Enqueue(rootDataSetId);
        visited.Add(rootDataSetId);

        // Breadth-first traversal
        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            result.Add(currentId);

            // Get the current dataset
            if (!dataSetLookup.TryGetValue(currentId, out var currentDataSet))
            {
                continue;
            }

            // Add all included datasets to the queue
            foreach (var include in currentDataSet.Includes)
            {
                var includedId = include.IncludedDataSetId;
                
                // Only add if not visited (prevents circular references)
                // and if the dataset exists in dataSetLookup
                if (!visited.Contains(includedId) && dataSetLookup.ContainsKey(includedId))
                {
                    visited.Add(includedId);
                    queue.Enqueue(includedId);
                }
            }
        }

        return result;
    }
}
