using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.DataSets;

/// <summary>
/// Query service for DataSet entities with authorization filtering
/// </summary>
public class DataSetsQueryService : QueryService<DataSet, Guid>
{
    private readonly DataManagerDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public DataSetsQueryService(
        DataManagerDbContext context,
        IFilterHandlerRegistry filterHandlerRegistry,
        IAuthorizationService authorizationService)
        : base(filterHandlerRegistry)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    protected override IQueryable<DataSet> DefaultQuery => _context.DataSets;

    /// <summary>
    /// Applies authorization filtering to ensure user only sees datasets they have access to.
    /// This method can be used for both list queries and single entity queries.
    /// </summary>
    public async Task<IQueryable<DataSet>> ApplyAuthorizationAsync(
        IQueryable<DataSet> query,
        CancellationToken cancellationToken = default)
    {
        // Get accessible dataset IDs from authorization service
        // This handles both root access and dataset-level permissions
        var (allAccessible, accessibleIds) = await _authorizationService.GetAccessibleDataSetsIdsAsync(cancellationToken);

        // If user has access to all datasets, no filtering needed
        if (allAccessible)
        {
            return query;
        }

        if (accessibleIds.Any())
        {
            // User has access to specific datasets - filter by those
            query = query.Where(ds => accessibleIds.Contains(ds.Id));
        }
        else
        {
            // User has no accessible datasets - return empty query
            // This ensures no data leakage even if authorization returns empty list
            query = query.Where(ds => false);
        }

        return query;
    }

    /// <summary>
    /// Prepares a query with authorization pre-filtering, applying filters, includes, and ordering.
    /// Only datasets the user has access to will be included.
    /// If query is null, uses DefaultQuery from DbContext.
    /// </summary>
    public override async Task<IQueryable<DataSet>> PrepareQueryAsync(
        IQueryable<DataSet>? query = null,
        QueryOptions<DataSet, Guid>? options = null,
        CancellationToken cancellationToken = default)
    {
        query = GetQuery(query);

        // Apply authorization pre-filter first
        query = await ApplyAuthorizationAsync(query, cancellationToken);

        // Call base implementation to apply filters, includes, and ordering
        return await base.PrepareQueryAsync(query, options, cancellationToken);
    }

    public class Options : QueryOptions<DataSet, Guid, DataSet>
    {
        public Options()
        {
            AsNoTracking = true;
        }
    }

    /// <summary>
    /// Gets all datasets in hierarchical order for a given root dataset ID.
    /// Returns a list starting with the root dataset, followed by all included datasets in breadth-first order.
    /// Example: If "Final" includes "GlobalData", and "GlobalData" includes "A" and "B",
    /// the result will be [Final, GlobalData, A, B].
    /// Handles circular references by tracking visited datasets.
    /// </summary>
    /// <param name="rootDataSetId">The ID of the root dataset to start traversal from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of dataset IDs in hierarchical order</returns>
    public async Task<List<Guid>> GetDataSetHierarchyIdsAsync(
        Guid rootDataSetId,
        CancellationToken cancellationToken = default)
    {
        var (hierarchyIds, _) = await GetDataSetHierarchyInternalAsync(rootDataSetId, cancellationToken);
        return hierarchyIds;
    }

    /// <summary>
    /// Gets all datasets in hierarchical order for a given root dataset ID, with full dataset details.
    /// Returns a list starting with the root dataset, followed by all included datasets in breadth-first order.
    /// Example: If "Final" includes "GlobalData", and "GlobalData" includes "A" and "B",
    /// the result will be [Final, GlobalData, A, B].
    /// Handles circular references by tracking visited datasets.
    /// </summary>
    /// <param name="rootDataSetId">The ID of the root dataset to start traversal from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of datasets in hierarchical order</returns>
    public async Task<List<DataSet>> GetDataSetHierarchyAsync(
        Guid rootDataSetId,
        CancellationToken cancellationToken = default)
    {
        var (hierarchyIds, dataSetLookup) = await GetDataSetHierarchyInternalAsync(rootDataSetId, cancellationToken);

        // Return datasets in the same order as hierarchy IDs
        var result = new List<DataSet>();
        foreach (var id in hierarchyIds)
        {
            if (dataSetLookup.TryGetValue(id, out var dataSet))
            {
                result.Add(dataSet);
            }
        }

        return result;
    }

    /// <summary>
    /// Internal method that performs the hierarchy traversal and returns both IDs and entities.
    /// This avoids duplicate database queries when both IDs and entities are needed.
    /// </summary>
    private async Task<(List<Guid> hierarchyIds, Dictionary<Guid, DataSet> dataSetLookup)> GetDataSetHierarchyInternalAsync(
        Guid rootDataSetId,
        CancellationToken cancellationToken = default)
    {
        // Fetch all datasets with their includes from the database
        // Apply authorization filtering to ensure user only sees accessible datasets
        var query = await ApplyAuthorizationAsync(_context.DataSets, cancellationToken);
        
        var allDataSets = await query
            .Include(ds => ds.Includes)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Create a lookup for quick access
        var dataSetLookup = allDataSets.ToDictionary(ds => ds.Id);

        // Check if the root dataset exists and is accessible
        if (!dataSetLookup.ContainsKey(rootDataSetId))
        {
            return (new List<Guid>(), dataSetLookup);
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
                // and if the dataset is accessible (exists in dataSetLookup)
                if (!visited.Contains(includedId) && dataSetLookup.ContainsKey(includedId))
                {
                    visited.Add(includedId);
                    queue.Enqueue(includedId);
                }
            }
        }

        return (result, dataSetLookup);
    }
}
