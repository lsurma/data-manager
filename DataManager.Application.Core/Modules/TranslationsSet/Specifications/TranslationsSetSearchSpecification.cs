using System.Linq.Expressions;
using DataManager.Application.Core.Common;

namespace DataManager.Application.Core.Modules.TranslationsSet.Specifications;

public class TranslationsSetSearchSpecification : SearchSpecification<TranslationsSet>
{
    public TranslationsSetSearchSpecification(string searchTerm) : base(searchTerm)
    {
    }

    public override Expression<Func<TranslationsSet, bool>> ToExpression()
    {
        return d => 
            d.Name.ToLower().Contains(SearchTerm) ||
            (d.Description != null && d.Description.ToLower().Contains(SearchTerm)) ||
            (d.Notes != null && d.Notes.ToLower().Contains(SearchTerm));
    }
}
