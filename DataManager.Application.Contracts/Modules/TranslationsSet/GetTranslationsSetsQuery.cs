using DataManager.Application.Contracts.Common;

namespace DataManager.Application.Contracts.Modules.TranslationsSet;

public class GetTranslationsSetsQuery : PaginatedQuery<TranslationsSetDto>
{
    /// <summary>
    /// Creates a query that returns all items without pagination
    /// </summary>
    public static GetTranslationsSetsQuery AllItems(string? orderBy = null, string? orderDirection = null)
    {
        return AllItems<GetTranslationsSetsQuery>(orderBy, orderDirection);
    }
}
