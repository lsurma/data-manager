using DataManager.Application.Contracts.Common;

namespace DataManager.Application.Contracts.Modules.Log;

public class GetLogsQuery : PaginatedQuery<LogDto>
{
    /// <summary>
    /// Creates a query that returns all items without pagination
    /// </summary>
    public static GetLogsQuery AllItems(string? orderBy = null, string? orderDirection = null)
    {
        return AllItems<GetLogsQuery>(orderBy, orderDirection);
    }
}
