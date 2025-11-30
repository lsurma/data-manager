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

public partial class TranslationsPage : ComponentBase, IDisposable
{
    [Inject] 
    private IDialogService DialogService { get; set; } = null!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;
    
    [Inject]
    private NavigationHelper NavHelper { get; set; } = null!;
    
    [Inject]
    private IRequestSender RequestSender { get; set; } = null!;
    
    private List<TranslationDto> AllTranslations { get; set; } = new();
    private List<DataSetDto> AllDataSets { get; set; } = new();
    private List<TranslationDto> AllLayouts { get; set; } = new();
    private IDialogReference? _currentDialog;
    private string _refreshToken = Guid.NewGuid().ToString();
    private IList<TranslationDto> _selectedRows = new List<TranslationDto>();
    private GetTranslationsQuery _currentQuery = new GetTranslationsQuery
    {
        Pagination = new PaginationParameters { PageNumber = 1, PageSize = 15 }
    };
    
    // Should stay static - we dont wanna cache all different queries separately
    private string _cacheKey = "paginated_translations";
    
    private int _totalItems;
    private int _pageSize = 20;
    private string? _searchTerm;
    private string? _cultureNameFilter;
    private Guid? _selectedTranslationId;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        LoadDataSetsAsync();
        LoadLayoutsAsync();
    }
    
    private async void LoadDataSetsAsync()
    {
        try
        {
            var result = await RequestSender.SendAsync(GetDataSetsQuery.AllItems());
            AllDataSets = result.Items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load data sets: {ex.Message}");
        }
    }
    
    private async void LoadLayoutsAsync()
    {
        try
        {
            // Load all translations that can be used as layouts
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
            // Load base translations (SourceId = null) that match the culture or have null culture
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
        
        Console.WriteLine($"Data fetched - IsFromCache: {eventArgs.IsFromCache}, IsFirstFetch: {eventArgs.IsFirstFetch}, Total: {_totalItems}, Page: {eventArgs.Data.CurrentPage}");
        
        RestoreDataGridSelection();
        
        // Process URL parameters on first live data fetch
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

            NavigationManager.NavigateTo($"/translations?id={translation.Id}", false);
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
        
        if (selectedTranslation != null)
        {
            _selectedRows = new List<TranslationDto> { selectedTranslation };
        }
        else
        {
            _selectedRows = new List<TranslationDto>();
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
        
        var hasFiltersChanged = GetCurrentSearchTerm() != _searchTerm ||
                                !HasSameCultureFilter(_currentQuery.Filtering.QueryFilters);

        if (_currentQuery.Ordering.OrderBy != orderBy ||
            _currentQuery.Ordering.OrderDirection != orderDirection ||
            _currentQuery.Pagination.Skip != skip ||
            _currentQuery.Pagination.PageSize != pageSize ||
            hasFiltersChanged)
        {
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
        var filters = new List<IQueryFilter>();

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

    private string? GetCurrentSearchTerm()
    {
        return _currentQuery.Filtering.QueryFilters
            .OfType<SearchFilter>()
            .FirstOrDefault()?.SearchTerm;
    }
    
    private bool HasSameCultureFilter(List<IQueryFilter> filters)
    {
        var existingCultureFilter = filters.OfType<CultureNameFilter>().FirstOrDefault();
        
        if (string.IsNullOrWhiteSpace(_cultureNameFilter))
        {
            return existingCultureFilter == null;
        }
        
        return existingCultureFilter?.Value == _cultureNameFilter;
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
        try
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
        }
        catch
        {
            
        }
        
        StateHasChanged();
    }
    
    private async Task OpenTranslationPanelAsync(TranslationDto? translation = null)
    {
        var isEditMode = translation != null;
        var cultureName = translation?.CultureName;
        
        // Load available sources filtered by the translation's culture (or null for new translations)
        var availableSources = await LoadSourcesForCultureAsync(cultureName);
        
        var parameters = new TranslationPanelParameters
        {
            Translation = isEditMode 
                ? translation! with { }
                : new TranslationDto
                {
                    Id = Guid.NewGuid(),
                    InternalGroupName1 = null,
                    InternalGroupName2 = null,
                    ResourceName = string.Empty,
                    TranslationName = string.Empty,
                    CultureName = null,
                    Content = string.Empty,
                    CreatedAt = DateTimeOffset.UtcNow
                },
            
            IsEditMode = isEditMode,
            AvailableDataSets = AllDataSets,
            AvailableLayouts = AllLayouts,
            AvailableSources = availableSources,
            
            OnDataChanged = async () =>
            {
                _refreshToken = Guid.NewGuid().ToString();
                LoadLayoutsAsync(); // Refresh layouts after data changes
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

        var result = await _currentDialog.Result;
        _currentDialog = null;
        var currentId = NavHelper.GetQueryParameter("id");
        
        if(result.Cancelled && currentId == translation?.Id.ToString())
        {
            NavigationManager.NavigateTo("/translations", false);
        }
        
        StateHasChanged();
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}
