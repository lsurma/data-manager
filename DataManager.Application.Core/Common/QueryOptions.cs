using System.Linq.Expressions;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Core.Abstractions;

namespace DataManager.Application.Core.Common;

/// <summary>
/// Options for configuring query preparation
/// </summary>
public class QueryOptions<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
    where TPrimaryKey : notnull
{
    /// <summary>
    /// Function to apply EF Core Include() chains for eager loading
    /// </summary>
    public Func<IQueryable<TEntity>, IQueryable<TEntity>>? IncludeFunc { get; set; }

    /// <summary>
    /// Type-safe include expressions for eager loading
    /// </summary>
    public List<Expression<Func<TEntity, object>>> Includes { get; set; } = new();

    /// <summary>
    /// Filtering parameters for the query
    /// </summary>
    public FilteringParameters? Filtering { get; set; }

    /// <summary>
    /// Ordering parameters for the query
    /// </summary>
    public OrderingParameters? Ordering { get; set; }

    /// <summary>
    /// Apply AsNoTracking to the query (default: false - tracking enabled)
    /// </summary>
    public bool AsNoTracking { get; set; } = false;

    /// <summary>
    /// Use DbContext Local view (change tracker) for fetching entities instead of querying the database.
    /// This is useful for optimizing bulk operations where entities are already loaded.
    /// When true, queries entities from the local change tracker with the same authorization
    /// and filtering logic applied in-memory.
    /// Note: This only works if entities are already tracked by the DbContext.
    /// </summary>
    public bool UseLocalView { get; set; } = false;

    /// <summary>
    /// Fluent API to add a type-safe include expression
    /// </summary>
    public QueryOptions<TEntity, TPrimaryKey> Include(Expression<Func<TEntity, object>> include)
    {
        Includes.Add(include);
        return this;
    }
}

/// <summary>
/// Options for configuring query preparation with projection/selector
/// </summary>
public class QueryOptions<TEntity, TPrimaryKey, TResult> : QueryOptions<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
    where TPrimaryKey : notnull
{
    /// <summary>
    /// Selector expression for database-level projection to TResult
    /// </summary>
    public Expression<Func<TEntity, TResult>>? Selector { get; set; }

    /// <summary>
    /// Fluent API to set the selector expression
    /// </summary>
    public QueryOptions<TEntity, TPrimaryKey, TResult> Select(Expression<Func<TEntity, TResult>> selector)
    {
        Selector = selector;
        return this;
    }
}
