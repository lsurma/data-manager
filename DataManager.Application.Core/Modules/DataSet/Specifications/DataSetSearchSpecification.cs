using System.Linq.Expressions;
using DataManager.Application.Core.Common;

namespace DataManager.Application.Core.Modules.DataSet.Specifications;

public class DataSetSearchSpecification : SearchSpecification<DataSet>
{
    public DataSetSearchSpecification(string searchTerm) : base(searchTerm)
    {
    }

    public override Expression<Func<DataSet, bool>> ToExpression()
    {
        return d => 
            d.Name.ToLower().Contains(SearchTerm) ||
            (d.Description != null && d.Description.ToLower().Contains(SearchTerm)) ||
            (d.Notes != null && d.Notes.ToLower().Contains(SearchTerm));
    }
}
