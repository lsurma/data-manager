using System.Linq.Expressions;
using DataManager.Application.Core.Common;

namespace DataManager.Application.Core.Modules.Translations.Specifications;

public class TranslationSearchSpecification : SearchSpecification<Translation>
{
    public TranslationSearchSpecification(string searchTerm) : base(searchTerm)
    {
    }

    public override Expression<Func<Translation, bool>> ToExpression()
    {
        return t => 
            (t.InternalGroupName1 != null && t.InternalGroupName1.ToLower().Contains(SearchTerm)) ||
            (t.InternalGroupName2 != null && t.InternalGroupName2.ToLower().Contains(SearchTerm)) ||
            t.ResourceName.ToLower().Contains(SearchTerm) ||
            t.TranslationName.ToLower().Contains(SearchTerm) ||
            t.Content.ToLower().Contains(SearchTerm);
    }
}
