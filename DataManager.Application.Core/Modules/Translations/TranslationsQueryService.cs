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
        var (allAccessible, accessibleIds) = await _authorizationService.GetAccessibleTranslationSetIdsAsync(cancellationToken);

        // If user has access to all datasets, no filtering needed
        if (allAccessible)
        {
            return query;
        }

        if (accessibleIds.Any())
        {
            // User has access to specific datasets - filter by those
            // Note: TranslationSetId is nullable, so we need to handle that
            query = query.Where(t => t.TranslationSetId.HasValue && accessibleIds.Contains(t.TranslationSetId.Value));
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
    /// Fetches translations from a TranslationSet hierarchy with deduplication based on priority.
    /// This is a CORE method that omits authorization - use with caution!
    /// 
    /// The method:
    /// 1. Traverses the TranslationSet hierarchy starting from root (breadth-first)
    /// 2. Fetches minimal data (Id + deduplication keys) from each dataset in hierarchy order
    /// 3. Applies deduplication: translations from higher priority datasets (earlier in hierarchy) 
    ///    take precedence over duplicates from lower priority datasets
    /// 4. Deduplication key: (ResourceName, CultureName, TranslationName)
    /// 5. Fetches full entities for deduplicated IDs in a single query
    /// 
    /// Use case: When a translation exists in the main dataset, any translation with the same
    /// (ResourceName, CultureName, TranslationName) from included datasets is ignored.
    /// </summary>
    /// <param name="rootTranslationSetId">The root TranslationSet ID to start hierarchy traversal from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of deduplicated translations respecting hierarchy priority</returns>
    public async Task<List<Translation>> GetTranslationsFromHierarchyAsync(
        Guid rootTranslationSetId,
        CancellationToken cancellationToken = default)
    {
        // Get the dataset hierarchy IDs in priority order (root first)
        // This is a core method without authorization
        var translationSetIds = await GetDataSetHierarchyIdsWithoutAuthorizationAsync(rootTranslationSetId, cancellationToken);

        if (!translationSetIds.Any())
        {
            return new List<Translation>();
        }

        // Phase 1: Collect IDs of translations that pass deduplication
        // Process translations dataset by dataset in hierarchy order (memory efficient)
        // Note: We're NOT applying authorization here as this is a core method
        var selectedIds = new List<Guid>();
        var seenKeys = new HashSet<(string ResourceName, string? CultureName, string TranslationName)>();

        // Fetch minimal data from each dataset in hierarchy order (respecting priority)
        foreach (var translationSetId in translationSetIds)
        {
            // Fetch only minimal data needed for deduplication (Id + key fields)
            var translationKeysForDataSet = await _context.Translations
                .Where(t => t.TranslationSetId == translationSetId)
                .Where(t => t.IsCurrentVersion) // Only current versions
                .Select(t => new
                {
                    t.Id,
                    t.ResourceName,
                    t.CultureName,
                    t.TranslationName
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var translationKey in translationKeysForDataSet)
            {
                var key = (translationKey.ResourceName, translationKey.CultureName, translationKey.TranslationName);
                
                // Only add if we haven't seen this key before (higher priority datasets were processed first)
                if (seenKeys.Add(key))
                {
                    selectedIds.Add(translationKey.Id);
                }
            }
        }

        // Phase 2: Fetch full entities for selected IDs in a single query
        if (!selectedIds.Any())
        {
            return new List<Translation>();
        }

        var result = await _context.Translations
            .Where(t => selectedIds.Contains(t.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Maintain the order based on hierarchy priority (order in selectedIds)
        var orderedResult = new List<Translation>();
        foreach (var id in selectedIds)
        {
            var translation = result.FirstOrDefault(t => t.Id == id);
            if (translation != null)
            {
                orderedResult.Add(translation);
            }
        }

        return orderedResult;
    }

    /// <summary>
    /// Materializes translations from the dataset hierarchy into the root dataset.
    /// This is a CORE method that omits authorization - use with caution!
    /// 
    /// Creates a "materialized view" by copying translations from included datasets into the root dataset.
    /// Translations that already exist in the root dataset are not copied (respecting hierarchy priority).
    /// Each copied translation is marked with SourceTranslationId (pointing to source) and SourceTranslationLastSyncedAt.
    /// 
    /// Example: If "some-custom-data-set" includes "data-set-a" and "data-set-b",
    /// this method will copy all translations from those datasets into "some-custom-data-set"
    /// so that a simple query on "some-custom-data-set" returns all translations without hierarchy traversal.
    /// </summary>
    /// <param name="rootTranslationSetId">The root TranslationSet ID to materialize translations into</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of translations materialized (added or updated)</returns>
    public async Task<int> MaterializeTranslationsFromHierarchyAsync(
        Guid rootTranslationSetId,
        CancellationToken cancellationToken = default)
    {
        // Get the dataset hierarchy IDs in priority order (root first)
        var translationSetIds = await GetDataSetHierarchyIdsWithoutAuthorizationAsync(rootTranslationSetId, cancellationToken);

        if (!translationSetIds.Any() || translationSetIds.Count == 1)
        {
            // No included datasets to materialize from
            return 0;
        }

        var syncTimestamp = DateTimeOffset.UtcNow;
        var materializedCount = 0;

        // Get existing translations in root dataset for deduplication
        var existingKeysInRoot = await _context.Translations
            .Where(t => t.TranslationSetId == rootTranslationSetId)
            .Where(t => t.IsCurrentVersion)
            .Select(t => new
            {
                Key = new { t.ResourceName, t.CultureName, t.TranslationName },
                t.Id,
                t.SourceTranslationId
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var existingKeys = new HashSet<(string ResourceName, string? CultureName, string TranslationName)>(
            existingKeysInRoot.Where(t => t.SourceTranslationId == null) // Only consider "original" translations, not previously materialized ones
                .Select(t => (t.Key.ResourceName, t.Key.CultureName, t.Key.TranslationName))
        );

        var existingMaterializedIds = new HashSet<Guid>(
            existingKeysInRoot.Where(t => t.SourceTranslationId.HasValue)
                .Select(t => t.Id)
        );

        // Process included datasets (skip the root dataset at index 0)
        for (int i = 1; i < translationSetIds.Count; i++)
        {
            var sourceTranslationSetId = translationSetIds[i];

            // Fetch translations from source dataset
            var translationsToMaterialize = await _context.Translations
                .Where(t => t.TranslationSetId == sourceTranslationSetId)
                .Where(t => t.IsCurrentVersion)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var sourceTranslation in translationsToMaterialize)
            {
                var key = (sourceTranslation.ResourceName, sourceTranslation.CultureName, sourceTranslation.TranslationName);

                // Skip if this translation key already exists as an original in root dataset
                if (existingKeys.Contains(key))
                {
                    continue;
                }

                // Check if we already have a materialized version
                var existingMaterialized = existingKeysInRoot
                    .FirstOrDefault(e => e.SourceTranslationId.HasValue &&
                                        e.Key.ResourceName == key.ResourceName &&
                                        e.Key.CultureName == key.CultureName &&
                                        e.Key.TranslationName == key.TranslationName);

                if (existingMaterialized != null)
                {
                    // Update existing materialized translation
                    var existingEntity = await _context.Translations
                        .FirstOrDefaultAsync(t => t.Id == existingMaterialized.Id, cancellationToken);

                    if (existingEntity != null)
                    {
                        // Update content and sync timestamp
                        existingEntity.Content = sourceTranslation.Content;
                        existingEntity.ContentTemplate = sourceTranslation.ContentTemplate;
                        existingEntity.InternalGroupName1 = sourceTranslation.InternalGroupName1;
                        existingEntity.InternalGroupName2 = sourceTranslation.InternalGroupName2;
                        existingEntity.LayoutId = sourceTranslation.LayoutId;
                        existingEntity.SourceTranslationId = sourceTranslation.Id; // Update to point to current source
                        existingEntity.SourceTranslationLastSyncedAt = syncTimestamp;
                        existingEntity.UpdatedAt = syncTimestamp;

                        materializedCount++;
                    }
                }
                else
                {
                    // Create new materialized translation
                    var materializedTranslation = new Translation
                    {
                        Id = Guid.NewGuid(),
                        ResourceName = sourceTranslation.ResourceName,
                        TranslationName = sourceTranslation.TranslationName,
                        CultureName = sourceTranslation.CultureName,
                        Content = sourceTranslation.Content,
                        ContentTemplate = sourceTranslation.ContentTemplate,
                        InternalGroupName1 = sourceTranslation.InternalGroupName1,
                        InternalGroupName2 = sourceTranslation.InternalGroupName2,
                        TranslationSetId = rootTranslationSetId,
                        SourceTranslationId = sourceTranslation.Id, // Point to source translation
                        SourceTranslationLastSyncedAt = syncTimestamp,
                        LayoutId = sourceTranslation.LayoutId,
                        SourceId = null, // Don't copy source reference
                        IsCurrentVersion = true,
                        IsDraftVersion = false,
                        IsOldVersion = false,
                        OriginalTranslationId = null,
                        CreatedAt = syncTimestamp,
                        UpdatedAt = syncTimestamp,
                        CreatedBy = "System.Materialization"
                    };

                    _context.Translations.Add(materializedTranslation);
                    existingKeys.Add(key); // Mark as seen to avoid duplicates from other datasets
                    materializedCount++;
                }
            }
        }

        // Save all changes
        if (materializedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return materializedCount;
    }

    /// <summary>
    /// Gets all datasets in hierarchical order for a given root dataset ID WITHOUT authorization.
    /// This is a CORE method - use with caution!
    /// Returns a list starting with the root dataset, followed by all included datasets in breadth-first order.
    /// Handles circular references by tracking visited datasets.
    /// </summary>
    /// <param name="rootTranslationSetId">The ID of the root dataset to start traversal from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of dataset IDs in hierarchical order</returns>
    private async Task<List<Guid>> GetDataSetHierarchyIdsWithoutAuthorizationAsync(
        Guid rootTranslationSetId,
        CancellationToken cancellationToken = default)
    {
        // Fetch all datasets with their includes from the database WITHOUT authorization
        // Performance note: This loads all datasets into memory. For most applications, 
        // the number of datasets is manageable (dozens to hundreds), making this approach acceptable.
        // If performance becomes an issue with thousands of datasets, consider using a recursive CTE
        // or fetching only datasets reachable from the root.
        var allTranslationSets = await _context.TranslationSets
            .Include(ds => ds.Includes)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Create a lookup for quick access
        var translationSetLookup = allTranslationSets.ToDictionary(ds => ds.Id);

        // Check if the root dataset exists
        if (!translationSetLookup.ContainsKey(rootTranslationSetId))
        {
            return new List<Guid>();
        }

        var result = new List<Guid>();
        var visited = new HashSet<Guid>();
        var queue = new Queue<Guid>();

        // Start with the root dataset
        queue.Enqueue(rootTranslationSetId);
        visited.Add(rootTranslationSetId);

        // Breadth-first traversal
        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            result.Add(currentId);

            // Get the current dataset
            if (!translationSetLookup.TryGetValue(currentId, out var currentDataSet))
            {
                continue;
            }

            // Add all included datasets to the queue
            foreach (var include in currentDataSet.Includes)
            {
                var includedId = include.IncludedTranslationSetId;
                
                // Only add if not visited (prevents circular references)
                // and if the dataset exists in translationSetLookup
                if (!visited.Contains(includedId) && translationSetLookup.ContainsKey(includedId))
                {
                    visited.Add(includedId);
                    queue.Enqueue(includedId);
                }
            }
        }

        return result;
    }
}
