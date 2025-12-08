using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;

namespace DataManager.Application.Core.Modules.Log;

/// <summary>
/// Query service for Log entities with authorization filtering
/// </summary>
public class LogsQueryService : QueryService<Log, Guid>
{
    private readonly DataManagerDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public LogsQueryService(
        DataManagerDbContext context,
        IFilterHandlerRegistry filterHandlerRegistry,
        IAuthorizationService authorizationService)
        : base(filterHandlerRegistry)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    protected override IQueryable<Log> DefaultQuery => _context.Logs;

    /// <summary>
    /// Applies authorization filtering to ensure only root users (or all users when auth is disabled) can access logs.
    /// </summary>
    public async Task<IQueryable<Log>> ApplyAuthorizationAsync(
        IQueryable<Log> query,
        CancellationToken cancellationToken = default)
    {
        // Check if user has root access
        var hasRootAccess = await _authorizationService.HasRootAccessAsync(cancellationToken);

        // Only root users (or all users when auth is disabled) can access logs
        if (!hasRootAccess)
        {
            // User does not have root access - return empty query
            query = query.Where(log => false);
        }

        return query;
    }

    /// <summary>
    /// Prepares a query with authorization pre-filtering, applying filters, includes, and ordering.
    /// Only root users (or all users when auth is disabled) can access logs.
    /// If query is null, uses DefaultQuery from DbContext.
    /// </summary>
    public override async Task<IQueryable<Log>> PrepareQueryAsync(
        IQueryable<Log>? query = null,
        QueryOptions<Log, Guid>? options = null,
        CancellationToken cancellationToken = default)
    {
        query = GetQuery(query);

        // Apply authorization pre-filter first
        query = await ApplyAuthorizationAsync(query, cancellationToken);

        // Call base implementation to apply filters, includes, and ordering
        return await base.PrepareQueryAsync(query, options, cancellationToken);
    }
}
