using DataManager.Application.Contracts.Common;

namespace DataManager.Application.Contracts.Modules.TranslationSet;

public class GetTranslationSetsQuery : PaginatedQuery<TranslationSetDto>
{
    /// <summary>
    /// Creates a query that returns all items without pagination
    /// </summary>
    public static GetTranslationSetsQuery AllItems(string? orderBy = null, string? orderDirection = null)
    {
        return AllItems<GetTranslationSetsQuery>(orderBy, orderDirection);
    }
}
