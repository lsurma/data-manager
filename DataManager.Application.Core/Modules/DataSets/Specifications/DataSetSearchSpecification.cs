using DataManager.Application.Core.Common;
using System.Linq.Expressions;

namespace DataManager.Application.Core.Modules.DataSets.Specifications;

public class TranslationSetSearchSpecification : SearchSpecification<DataSet>
{
    public TranslationSetSearchSpecification(string searchTerm) : base(searchTerm)
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
