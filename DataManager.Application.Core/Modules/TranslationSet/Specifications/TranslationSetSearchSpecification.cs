using System.Linq.Expressions;
using DataManager.Application.Core.Common;

namespace DataManager.Application.Core.Modules.TranslationSet.Specifications;

public class TranslationSetSearchSpecification : SearchSpecification<TranslationSet>
{
    public TranslationSetSearchSpecification(string searchTerm) : base(searchTerm)
    {
    }

    public override Expression<Func<TranslationSet, bool>> ToExpression()
    {
        return d => 
            d.Name.ToLower().Contains(SearchTerm) ||
            (d.Description != null && d.Description.ToLower().Contains(SearchTerm)) ||
            (d.Notes != null && d.Notes.ToLower().Contains(SearchTerm));
    }
}
