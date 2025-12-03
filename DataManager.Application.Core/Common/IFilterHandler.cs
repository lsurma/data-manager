using System.Linq.Expressions;
using DataManager.Application.Contracts.Common;

namespace DataManager.Application.Core.Common;

/// <summary>
/// Handler for applying a specific filter to entity queries.
/// Each filter type has its own handler implementation.
/// Supports async operations for filters that require database lookups or other async operations.
/// </summary>
public interface IFilterHandler<TEntity, TFilter> : IFilterHandler  
    where TFilter : IQueryFilter
{
    Task<Expression<Func<TEntity, bool>>> GetFilterExpressionAsync(TFilter filter, CancellationToken cancellationToken = default);
}

/// <summary>
/// Marker interface for filter handlers.
/// </summary>
public interface IFilterHandler
{
    
}