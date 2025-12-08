using DataManager.Application.Contracts.Common;
using DataManager.Application.Core.Common;
using System.Linq.Expressions;

namespace DataManager.Application.Core.Modules.DataSets.Filters;

public class DataSetSearchFilterHandler : IFilterHandler<DataSet, SearchFilter>
{
    public Task<Expression<Func<DataSet, bool>>> GetFilterExpressionAsync(SearchFilter filter, CancellationToken cancellationToken = default)
    {
        var searchTerm = filter.SearchTerm!.ToLower(); // We know it has value because IsActive() was checked
        Expression<Func<DataSet, bool>> expression = d =>
            d.Name.ToLower().Contains(searchTerm) ||
            (d.Description != null && d.Description.ToLower().Contains(searchTerm)) ||
            (d.Notes != null && d.Notes.ToLower().Contains(searchTerm));
        return Task.FromResult(expression);
    }
}
