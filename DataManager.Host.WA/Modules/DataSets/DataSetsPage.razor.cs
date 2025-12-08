using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.DataSets;
using DataManager.Host.WA.Components;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;
using System.Web;

namespace DataManager.Host.WA.Modules.DataSets;

public partial class DataSetsPage : ComponentBase, IDisposable
{
    [CascadingParameter]
    public AppDataContext? CascadingAppContext { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; } = null!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;
    
    [Inject]
    private NavigationHelper NavHelper { get; set; } = null!;
    
    private List<DataSetDto> AllDataSets { get; set; } = new();
    private IDialogReference? CurrentDialog { get; set; }
    private string RefreshToken { get; set; } = Guid.NewGuid().ToString();
    private IList<DataSetDto> SelectedRows { get; set; } = new List<DataSetDto>();
    
    private int PageSize { get; set; } = 15;
    
    protected int TotalItems { get; set; }
    
    private string? SearchTerm { get; set; }
    private Guid? SelectedDataSetId { get; set; }
    
    private GetDataSetsQuery CurrentQuery { get; set; } = new GetDataSetsQuery
    {
        Pagination = new PaginationParameters { PageNumber = 1, PageSize = 15 }
    };

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
    }
    
    private void HandleDataFetched(DataFetchedEventArgs<PaginatedList<DataSetDto>> eventArgs)
    {
        AllDataSets = eventArgs.Data.Items;
        TotalItems = eventArgs.Data.TotalItems;
        PageSize = eventArgs.Data.PageSize;
        
        Console.WriteLine($"Data fetched - IsFromCache: {eventArgs.IsFromCache}, IsFirstFetch: {eventArgs.IsFirstFetch}, Total: {TotalItems}, Page: {eventArgs.Data.CurrentPage}");
        
        RestoreDataGridSelection();
        
        // Process URL parameters on first live data fetch
        if (eventArgs.IsFirstFetch && !eventArgs.IsFromCache)
        {
            _ = ProcessUrlParametersAsync();
        }

        StateHasChanged();
    }
    
    private Task OnDataGridSelectionChanged(IList<DataSetDto> selectedRows)
    {
        SelectedRows = selectedRows;
        
        if (selectedRows != null && selectedRows.Count > 0)
        {
            var dataSet = selectedRows[0];
            SelectedDataSetId = dataSet.Id;

            NavigationManager.NavigateTo($"/data-sets?id={dataSet.Id}", false);
        }
        else
        {
            SelectedDataSetId = null;
        }
        
        return Task.CompletedTask;
    }
    
    private void RestoreDataGridSelection()
    {
        if (!SelectedDataSetId.HasValue)
        {
            SelectedRows = new List<DataSetDto>();
            return;
        }
        
        // Try to find in current grid page first, fallback to AllDataSets if not found
        var selectedDataSet = AllDataSets.FirstOrDefault(i => i.Id == SelectedDataSetId.Value);
        
        if (selectedDataSet != null)
        {
            SelectedRows = new List<DataSetDto> { selectedDataSet };
        }
        else
        {
            SelectedRows = new List<DataSetDto>();
        }
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
        var pageSize = args.Top ?? 20;

        if (CurrentQuery.Ordering.OrderBy != orderBy ||
            CurrentQuery.Ordering.OrderDirection != orderDirection ||
            CurrentQuery.Pagination.Skip != skip ||
            CurrentQuery.Pagination.PageSize != pageSize ||
            GetCurrentSearchTerm() != SearchTerm)
        {
            CurrentQuery = new GetDataSetsQuery
            {
                Filtering = BuildFilteringParameters(),
                Ordering = new OrderingParameters { OrderBy = orderBy, OrderDirection = orderDirection },
                Pagination = new PaginationParameters { Skip = skip, PageSize = pageSize }
            };

            RefreshToken = Guid.NewGuid().ToString();
        }
    }

    private void OnSearchChanged()
    {
        CurrentQuery = new GetDataSetsQuery
        {
            Filtering = BuildFilteringParameters(),
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

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        await ProcessUrlParametersAsync();
    }
    
    private async Task ProcessUrlParametersAsync()
    {
        try
        {
            var uri = new Uri(NavigationManager.Uri);
            var query = HttpUtility.ParseQueryString(uri.Query);
            var action = query["action"];
            var idParam = query["id"];
            
            if (action == "create")
            {
                SelectedDataSetId = null;
                await OpenDataSetPanelAsync();
            }
            else if (!string.IsNullOrEmpty(idParam) && Guid.TryParse(idParam, out var dataSetId))
            {
                SelectedDataSetId = dataSetId;
                var dataSet = AllDataSets.FirstOrDefault(i => i.Id == dataSetId);
                
                if (dataSet != null)
                {
                    await OpenDataSetPanelAsync(dataSet);
                }
            }
            else
            {
                SelectedDataSetId = null;
                
                if (CurrentDialog != null)
                {
                    await CurrentDialog.CloseAsync();
                    CurrentDialog = null;
                }
            }
        }
        catch
        {
            
        }
        
        StateHasChanged();
    }
    
    private async Task OpenDataSetPanelAsync(DataSetDto? dataSet = null)
    {
        var isEditMode = dataSet != null;

        // Use DataSets from AppContext
        var availableDataSets = CascadingAppContext?.DataSets;

        var parameters = new DataSetPanelParameters
        {
            DataSet = isEditMode 
                ? dataSet! with { }
                : new DataSetDto
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTimeOffset.UtcNow
                },

            IsEditMode = isEditMode,
            
            AvailableDataSets = isEditMode 
                ? availableDataSets?.Where(i => i.Id != dataSet!.Id).ToList() ?? []
                : availableDataSets ?? [],
            
            OnDataChanged = async () =>
            {
                RefreshToken = Guid.NewGuid().ToString();

                // Refresh context to get updated DataSets
                if (CascadingAppContext != null)
                {
                    await CascadingAppContext.RefreshAsync();
                }

                await InvokeAsync(StateHasChanged);
            }
        };

        var newDialog = await DialogService.ShowPanelAsync<DataSetPanel>(parameters, new DialogParameters
        {
            Title = isEditMode ? "Edit Data Set" : "Create New Data Set",
            Width = "600px",
            TrapFocus = false,
            Modal = false,
            Id = $"panel-{Guid.NewGuid()}"
        });
        
        if (CurrentDialog != null)
        {
            await CurrentDialog.CloseAsync();
        }
        
        CurrentDialog = newDialog;

        var result = await CurrentDialog.Result;
        CurrentDialog = null;
        var currentId = NavHelper.GetQueryParameter("id");
        
        if(result.Cancelled && currentId == dataSet?.Id.ToString())
        {
            NavigationManager.NavigateTo("/data-sets", false);
        }
        
        StateHasChanged();
    }
    
    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}
