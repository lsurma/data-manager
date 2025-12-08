using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.Log;
using DataManager.Host.WA.Components;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace DataManager.Host.WA.Modules.Logs;

public partial class LogsPage : ComponentBase
{
    private List<LogDto> AllLogs { get; set; } = new();
    private string RefreshToken { get; set; } = Guid.NewGuid().ToString();
    
    private int PageSize { get; set; } = 30;
    
    protected int TotalItems { get; set; }
    
    private string? SearchTerm { get; set; }
    
    private GetLogsQuery CurrentQuery { get; set; } = new GetLogsQuery
    {
        Pagination = new PaginationParameters { PageNumber = 1, PageSize = 30 },
        Ordering = new OrderingParameters { OrderBy = "StartedAt", OrderDirection = "desc" }
    };
    
    private void HandleDataFetched(DataFetchedEventArgs<PaginatedList<LogDto>> eventArgs)
    {
        AllLogs = eventArgs.Data.Items;
        TotalItems = eventArgs.Data.TotalItems;
        PageSize = eventArgs.Data.PageSize;
        
        StateHasChanged();
    }
    
    private void OnLoadData(LoadDataArgs args)
    {
        string? orderBy = null;
        string? orderDirection = null;

        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            var orderByParts = args.OrderBy.Split(' ');
            orderBy = orderByParts[0];
            orderDirection = orderByParts.Length > 1 && orderByParts[1].ToLower() == "desc" ? "desc" : "asc";
        }

        var skip = args.Skip ?? 0;
        var pageSize = args.Top ?? 30;
        var currentSearchTerm = GetCurrentSearchTerm();

        if (CurrentQuery.Ordering.OrderBy != orderBy ||
            CurrentQuery.Ordering.OrderDirection != orderDirection ||
            CurrentQuery.Pagination.Skip != skip ||
            CurrentQuery.Pagination.PageSize != pageSize ||
            currentSearchTerm != SearchTerm)
        {
            CurrentQuery = new GetLogsQuery
            {
                Filtering = BuildFilteringParameters(),
                Ordering = new OrderingParameters { OrderBy = orderBy ?? "StartedAt", OrderDirection = orderDirection ?? "desc" },
                Pagination = new PaginationParameters { Skip = skip, PageSize = pageSize }
            };

            RefreshToken = Guid.NewGuid().ToString();
        }
    }

    private void OnSearchChanged()
    {
        CurrentQuery = new GetLogsQuery
        {
            Filtering = BuildFilteringParameters(),
            Ordering = new OrderingParameters { OrderBy = "StartedAt", OrderDirection = "desc" },
            Pagination = new PaginationParameters { Skip = 0, PageSize = PageSize }
        };

        RefreshToken = Guid.NewGuid().ToString();
    }

    private FilteringParameters BuildFilteringParameters()
    {
        var filters = new List<IQueryFilter>();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            filters.Add(new SearchFilter { SearchTerm = SearchTerm });
        }

        return new FilteringParameters { QueryFilters = filters };
    }

    private string? GetCurrentSearchTerm()
    {
        return CurrentQuery.Filtering.QueryFilters
            .OfType<SearchFilter>()
            .FirstOrDefault()?.SearchTerm;
    }
}
