using DataManager.Application.Contracts.Common;

namespace DataManager.Application.Contracts.Modules.DataSets;

public class GetDataSetsQuery : PaginatedQuery<DataSetDto>
{
    /// <summary>
    /// Creates a query that returns all items without pagination
    /// </summary>
    public static GetDataSetsQuery AllItems(string? orderBy = null, string? orderDirection = null)
    {
        return AllItems<GetDataSetsQuery>(orderBy, orderDirection);
    }
}
