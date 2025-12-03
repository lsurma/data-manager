using System.Web;
using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.DataSet;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Host.WA.Components;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;

namespace DataManager.Host.WA.Modules.Translations;

public partial class TranslationsGrid : ComponentBase, IDisposable
{
    [Parameter]
    public List<IQueryFilter> Filters { get; set; } = new();

    [CascadingParameter(Name = "AppDataContext")]
    public AppDataContext AppContext { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private NavigationHelper NavHelper { get; set; } = null!;

    [Inject]
    private IRequestSender RequestSender { get; set; } = null!;

    private List<TranslationDto> AllTranslations { get; set; } = new();
    private List<DataSetDto> AllDataSets => AppContext?.DataSets ?? new List<DataSetDto>();
    private List<TranslationDto> AllLayouts { get; set; } = new();
    private IDialogReference? _currentDialog;
    private string _refreshToken = Guid.NewGuid().ToString();
    private IList<TranslationDto> _selectedRows = new List<TranslationDto>();
    private GetTranslationsQuery _currentQuery = new GetTranslationsQuery
    {
        Pagination = new PaginationParameters { PageNumber = 1, PageSize = 15 }
    };

    private string _cacheKey = "paginated_translations";
    private int _totalItems;
    private int _pageSize = 20;
    private string? _searchTerm;
    private string? _cultureNameFilter;
    private Guid? _selectedTranslationId;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        LoadLayoutsAsync();
        
        // Subscribe to context refresh events
        if (AppContext != null)
        {
            AppContext.OnDataRefreshed += HandleContextRefreshed;
        }
    }

    protected override void OnParametersSet()
    {
        _currentQuery = new GetTranslationsQuery
        {
            Filtering = new FilteringParameters
            {
                QueryFilters = BuildQueryFilters()
            },
            Pagination = new PaginationParameters { Skip = 0, PageSize = _pageSize }
        };
        _refreshToken = Guid.NewGuid().ToString();
    }

    private void HandleContextRefreshed()
    {
        StateHasChanged();
    }

    private async void LoadLayoutsAsync()
    {
        try
        {
            var result = await RequestSender.SendAsync(GetTranslationsQuery.AllItems());
            AllLayouts = result.Items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load layouts: {ex.Message}");
        }
    }

    private async Task<List<TranslationDto>> LoadSourcesForCultureAsync(string? cultureName)
    {
        try
        {
            var query = GetTranslationsQuery.AllItems();
            query.Filtering = new FilteringParameters
            {
                QueryFilters = new List<IQueryFilter>
                {
                    new BaseTranslationFilter { CultureName = cultureName }
                }
            };
            var result = await RequestSender.SendAsync(query);
            return result.Items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load sources: {ex.Message}");
            return new List<TranslationDto>();
        }
    }

    private void HandleDataFetched(DataFetchedEventArgs<PaginatedList<TranslationDto>> eventArgs)
    {
        AllTranslations = eventArgs.Data.Items;
        _totalItems = eventArgs.Data.TotalItems;
        _pageSize = eventArgs.Data.PageSize;
        RestoreDataGridSelection();

        if (eventArgs.IsFirstFetch && !eventArgs.IsFromCache)
        {
            _ = ProcessUrlParametersAsync();
        }

        StateHasChanged();
    }

    private Task OnDataGridSelectionChanged(IList<TranslationDto> selectedRows)
    {
        _selectedRows = selectedRows;
        if (selectedRows != null && selectedRows.Count > 0)
        {
            var translation = selectedRows[0];
            _selectedTranslationId = translation.Id;
            NavigationManager.NavigateTo(NavHelper.BuildUrl("id", translation.Id.ToString()), false);
        }
        else
        {
            _selectedTranslationId = null;
        }
        return Task.CompletedTask;
    }

    private void RestoreDataGridSelection()
    {
        if (!_selectedTranslationId.HasValue)
        {
            _selectedRows = new List<TranslationDto>();
            return;
        }
        var selectedTranslation = AllTranslations.FirstOrDefault(t => t.Id == _selectedTranslationId.Value);
        _selectedRows = selectedTranslation != null ? new List<TranslationDto> { selectedTranslation } : new List<TranslationDto>();
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

        _currentQuery = new GetTranslationsQuery
        {
            Filtering = new FilteringParameters
            {
                QueryFilters = BuildQueryFilters()
            },
            Ordering = new OrderingParameters { OrderBy = orderBy, OrderDirection = orderDirection },
            Pagination = new PaginationParameters { Skip = skip, PageSize = pageSize }
        };
        _refreshToken = Guid.NewGuid().ToString();
    }

    private void OnSearchChanged()
    {
        _currentQuery = new GetTranslationsQuery
        {
            Filtering = new FilteringParameters
            {
                QueryFilters = BuildQueryFilters()
            },
            Pagination = new PaginationParameters { Skip = 0, PageSize = _pageSize }
        };
        _refreshToken = Guid.NewGuid().ToString();
    }

    private void OnCultureFilterChanged()
    {
        _currentQuery = new GetTranslationsQuery
        {
            Filtering = new FilteringParameters
            {
                QueryFilters = BuildQueryFilters()
            },
            Pagination = new PaginationParameters { Skip = 0, PageSize = _pageSize }
        };
        _refreshToken = Guid.NewGuid().ToString();
    }

    private List<IQueryFilter> BuildQueryFilters()
    {
        var filters = new List<IQueryFilter>(Filters);
        if (!string.IsNullOrWhiteSpace(_searchTerm))
        {
            filters.Add(new SearchFilter { SearchTerm = _searchTerm });
        }
        if (!string.IsNullOrWhiteSpace(_cultureNameFilter))
        {
            filters.Add(new CultureNameFilter { Value = _cultureNameFilter });
        }
        return filters;
    }

    private void ClearCultureFilter()
    {
        _cultureNameFilter = null;
        OnCultureFilterChanged();
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        await ProcessUrlParametersAsync();
    }

    private async Task ProcessUrlParametersAsync()
    {
        var uri = new Uri(NavigationManager.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);
        var action = query["action"];
        var idParam = query["id"];

        if (action == "create")
        {
            _selectedTranslationId = null;
            await OpenTranslationPanelAsync();
        }
        else if (!string.IsNullOrEmpty(idParam) && Guid.TryParse(idParam, out var translationId))
        {
            _selectedTranslationId = translationId;
            var translation = AllTranslations.FirstOrDefault(t => t.Id == translationId);
            if (translation != null)
            {
                await OpenTranslationPanelAsync(translation);
            }
        }
        else
        {
            _selectedTranslationId = null;
            if (_currentDialog != null)
            {
                await _currentDialog.CloseAsync();
                _currentDialog = null;
            }
        }
        StateHasChanged();
    }

    private async Task OpenTranslationPanelAsync(TranslationDto? translation = null)
    {
        var isEditMode = translation != null;
        var parameters = new TranslationPanelParameters
        {
            Translation = isEditMode ? translation! : new TranslationDto { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow },
            IsEditMode = isEditMode,
            AvailableDataSets = AllDataSets,
            AvailableLayouts = AllLayouts,
            OnDataChanged = async () =>
            {
                _refreshToken = Guid.NewGuid().ToString();
                LoadLayoutsAsync();
                await InvokeAsync(StateHasChanged);
            }
        };

        var newDialog = await DialogService.ShowPanelAsync<TranslationPanel>(parameters, new DialogParameters
        {
            Title = isEditMode ? "Edit Translation" : "Create New Translation",
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
        
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        
        // Unsubscribe from context refresh events
        if (AppContext != null)
        {
            AppContext.OnDataRefreshed -= HandleContextRefreshed;
        }
    }
}
