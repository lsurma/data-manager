using System.Web;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.DataSet;
using DataManager.Host.WA.Components;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;

namespace DataManager.Host.WA.Modules.DataSets;

public partial class DataSetsPage : ComponentBase, IDisposable
{
    [CascadingParameter]
    public AppDataContext AppContext { get; set; } = null!;

    [Inject] 
    private IDialogService DialogService { get; set; } = null!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;
    
    [Inject]
    private NavigationHelper NavHelper { get; set; } = null!;
    
    private List<DataSetDto> AllDataSets { get; set; } = new();
    private IDialogReference? _currentDialog;
    private string _refreshToken = Guid.NewGuid().ToString();
    private IList<DataSetDto> _selectedRows = new List<DataSetDto>();
    
    private int PageSize { get; set; } = 15;
    
    protected int TotalItems { get; set; }
    
    private string? _searchTerm;
    private Guid? _selectedDataSetId;
    
    private GetDataSetsQuery _currentQuery = new GetDataSetsQuery
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
        _selectedRows = selectedRows;
        
        if (selectedRows != null && selectedRows.Count > 0)
        {
            var dataSet = selectedRows[0];
            _selectedDataSetId = dataSet.Id;

            NavigationManager.NavigateTo($"/datasets?id={dataSet.Id}", false);
        }
        else
        {
            _selectedDataSetId = null;
        }
        
        return Task.CompletedTask;
    }
    
    private void RestoreDataGridSelection()
    {
        if (!_selectedDataSetId.HasValue)
        {
            _selectedRows = new List<DataSetDto>();
            return;
        }
        
        // Try to find in current grid page first, fallback to AllDataSets if not found
        var selectedDataSet = AllDataSets.FirstOrDefault(i => i.Id == _selectedDataSetId.Value);
        
        if (selectedDataSet != null)
        {
            _selectedRows = new List<DataSetDto> { selectedDataSet };
        }
        else
        {
            _selectedRows = new List<DataSetDto>();
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

        if (_currentQuery.Ordering.OrderBy != orderBy ||
            _currentQuery.Ordering.OrderDirection != orderDirection ||
            _currentQuery.Pagination.Skip != skip ||
            _currentQuery.Pagination.PageSize != pageSize ||
            GetCurrentSearchTerm() != _searchTerm)
        {
            _currentQuery = new GetDataSetsQuery
            {
                Filtering = BuildFilteringParameters(),
                Ordering = new OrderingParameters { OrderBy = orderBy, OrderDirection = orderDirection },
                Pagination = new PaginationParameters { Skip = skip, PageSize = pageSize }
            };

            _refreshToken = Guid.NewGuid().ToString();
        }
    }

    private void OnSearchChanged()
    {
        _currentQuery = new GetDataSetsQuery
        {
            Filtering = BuildFilteringParameters(),
            Pagination = new PaginationParameters { Skip = 0, PageSize = PageSize }
        };

        _refreshToken = Guid.NewGuid().ToString();
    }

    private FilteringParameters BuildFilteringParameters()
    {
        var filters = new List<IQueryFilter>();

        if (!string.IsNullOrWhiteSpace(_searchTerm))
        {
            filters.Add(new SearchFilter { SearchTerm = _searchTerm });
        }

        return new FilteringParameters { QueryFilters = filters };
    }

    private string? GetCurrentSearchTerm()
    {
        return _currentQuery.Filtering.QueryFilters
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
                _selectedDataSetId = null;
                await OpenDataSetPanelAsync();
            }
            else if (!string.IsNullOrEmpty(idParam) && Guid.TryParse(idParam, out var dataSetId))
            {
                _selectedDataSetId = dataSetId;
                var dataSet = AllDataSets.FirstOrDefault(i => i.Id == dataSetId);
                
                if (dataSet != null)
                {
                    await OpenDataSetPanelAsync(dataSet);
                }
            }
            else
            {
                _selectedDataSetId = null;
                
                if (_currentDialog != null)
                {
                    await _currentDialog.CloseAsync();
                    _currentDialog = null;
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
        var availableDataSets = AppContext?.DataSets ?? new List<DataSetDto>();
        
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
                ? availableDataSets.Where(i => i.Id != dataSet!.Id).ToList()
                : availableDataSets,
            
            OnDataChanged = async () =>
            {
                _refreshToken = Guid.NewGuid().ToString();
                
                // Refresh context to get updated DataSets
                if (AppContext != null)
                {
                    await AppContext.RefreshAsync();
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
        
        if (_currentDialog != null)
        {
            await _currentDialog.CloseAsync();
        }
        
        _currentDialog = newDialog;

        var result = await _currentDialog.Result;
        _currentDialog = null;
        var currentId = NavHelper.GetQueryParameter("id");
        
        if(result.Cancelled && currentId == dataSet?.Id.ToString())
        {
            NavigationManager.NavigateTo("/datasets", false);
        }
        
        StateHasChanged();
    }
    
    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}
