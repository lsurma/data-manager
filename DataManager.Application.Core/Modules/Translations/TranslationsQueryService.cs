using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;

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
}
