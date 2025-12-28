using System.Web;
using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Contracts.Modules.DataSets;
using DataManager.Host.WA.Components;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;

namespace DataManager.Host.WA.Modules.Translations;

public partial class TranslationsGrid : ComponentBase, IDisposable
{
    [Parameter]
    public Guid? DataSetId { get; set; }

    [Parameter]
    public RenderFragment ToolbarTemplate { get; set; } = null!;

    [Parameter]
    public bool DisableAutoPanelOpening { get; set; } = false;

    [Parameter]
    public List<IQueryFilter> AdditionalFilters { get; set; } = new();

    [CascadingParameter]
    public AppDataContext? CascadingAppContext { get; set; }

    [Inject]
    private AppDataContext InjectedAppContext { get; set; } = null!;

    /// <summary>
    /// Gets the AppDataContext from cascading parameter if available, otherwise uses injected service
    /// </summary>
    private AppDataContext AppContext => CascadingAppContext ?? InjectedAppContext;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private NavigationHelper NavHelper { get; set; } = null!;

    [Inject]
    private IRequestSender RequestSender { get; set; } = null!;

    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = null!;

    [Inject]
    private ILogger<TranslationsGrid> Logger { get; set; } = null!;

    private List<TranslationDto> AllTranslations { get; set; } = new();
    private List<DataSetDto> AllDataSets => AppContext.DataSets;
    private List<TranslationDto> AllLayouts { get; set; } = new();
    private IDialogReference? CurrentDialog;
    private string? RefreshToken { get; set; }
    private IList<TranslationDto> SelectedRows = new List<TranslationDto>();
    private GetTranslationsQuery _currentQuery = new GetTranslationsQuery
    {
        Pagination = new PaginationParameters(0, 10)
    };

    private int TotalItems;
    private int PageSize = 10;
    private string? SearchTerm;
    private string? CultureNameFilter;
    private Guid? SelectedTranslationId;
    private bool ShowNotFilledOnly = false;
    
    // Filter-related properties
    private List<string> AvailableCultures { get; set; } = new();
    private AppFilterSettings FilterSettings { get; set; } = new();
    private const string FilterStorageKey = "translations-filter-settings";

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            try
            {
                await LoadAvailableCulturesAsync();
                await LoadFilterSettingsAsync();
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading cultures and filter settings");
            }
        }
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var dataSetIdChanged = parameters.TryGetValue<Guid?>(nameof(DataSetId), out var newDataSetId) && newDataSetId != DataSetId;
        if (dataSetIdChanged)
        {
            DataSetId = newDataSetId;
            _currentQuery = new GetTranslationsQuery
            {
                Filtering = new FilteringParameters
                {
                    QueryFilters = BuildQueryFilters()
                },
                Pagination = new PaginationParameters(0, PageSize)
            };
            RefreshToken = Guid.NewGuid().ToString();
        }
        
        await base.SetParametersAsync(parameters);
    }

    private void HandleContextRefreshed()
    {
        StateHasChanged();
    }

    private async void LoadLayoutsAsync()
    {
        try
        {
            // var result = await RequestSender.SendAsync(GetTranslationsQuery.AllItems());
            // AllLayouts = result.Items;
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
        TotalItems = eventArgs.Data.TotalItems;
        PageSize = eventArgs.Data.PageSize;
        RestoreDataGridSelection();

        if (eventArgs.IsFirstFetch && !eventArgs.IsFromCache)
        {
            _ = ProcessUrlParametersAsync();
        }

        StateHasChanged();
    }

    private Task OnDataGridSelectionChanged(IList<TranslationDto> selectedRows)
    {
        SelectedRows = selectedRows;
        if (selectedRows != null && selectedRows.Count > 0)
        {
            var translation = selectedRows[0];
            SelectedTranslationId = translation.Id;
            NavigationManager.NavigateTo(NavHelper.BuildUrl("id", translation.Id.ToString()), false);
        }
        else
        {
            SelectedTranslationId = null;
        }
        return Task.CompletedTask;
    }

    private void RestoreDataGridSelection()
    {
        if (!SelectedTranslationId.HasValue)
        {
            SelectedRows = new List<TranslationDto>();
            return;
        }
        var selectedTranslation = AllTranslations.FirstOrDefault(t => t.Id == SelectedTranslationId.Value);
        SelectedRows = selectedTranslation != null ? new List<TranslationDto> { selectedTranslation } : new List<TranslationDto>();
    }

    private void OnLoadData(LoadDataArgs args)
    {
        Console.WriteLine("OnLoadData called with args: " + args);
        string? orderBy = null;
        string? orderDirection = null;

        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            var orderByParts = args.OrderBy.Split(' ');
            orderBy = orderByParts[0];
            orderDirection = orderByParts.Length > 1 && orderByParts[1].ToLower() == "desc" ? "desc" : "asc";
        }

        var skip = args.Skip ?? 0;
        var pageSize = args.Top ?? 10;

        _currentQuery = new GetTranslationsQuery
        {
            Filtering = new FilteringParameters
            {
                QueryFilters = BuildQueryFilters()
            },
            Ordering = new OrderingParameters { OrderBy = orderBy, OrderDirection = orderDirection },
            Pagination = new PaginationParameters(skip, pageSize)
        };
        RefreshToken = Guid.NewGuid().ToString();
    }

    private void OnSearchChanged()
    {
        RefreshQueryWithFilters();
    }

    private void OnCultureFilterChanged()
    {
        RefreshQueryWithFilters();
    }

    private void OnNotFilledFilterChanged()
    {
        RefreshQueryWithFilters();
    }

    private void RefreshQueryWithFilters()
    {
        _currentQuery = new GetTranslationsQuery
        {
            Filtering = new FilteringParameters
            {
                QueryFilters = BuildQueryFilters()
            },
            Pagination = new PaginationParameters(0, PageSize)
        };
        RefreshToken = Guid.NewGuid().ToString();
    }

    private List<IQueryFilter> BuildQueryFilters()
    {
        var filters = new List<IQueryFilter>();
        
        // Add additional filters first (e.g., InternalGroupName1Filter from EmailsPage)
        if (AdditionalFilters.Count > 0)
        {
            filters.AddRange(AdditionalFilters);
        }
        
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            filters.Add(new SearchFilter { SearchTerm = SearchTerm });
        }
        
        if (!string.IsNullOrWhiteSpace(CultureNameFilter))
        {
            filters.Add(new CultureNameFilter { Value = CultureNameFilter });
        }
        
        if(DataSetId != null)
        {
            filters.Add(new DataSetIdFilter { Value = DataSetId.Value });
        }
        
        if (ShowNotFilledOnly)
        {
            filters.Add(new NotFilledFilter { Value = true });
        }
        
        return filters;
    }

    private void ClearCultureFilter()
    {
        CultureNameFilter = null;
        OnCultureFilterChanged();
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        await ProcessUrlParametersAsync();
    }

    private async Task ProcessUrlParametersAsync()
    {
        // Skip panel opening if disabled
        if (DisableAutoPanelOpening)
        {
            return;
        }

        var uri = new Uri(NavigationManager.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);
        var action = query["action"];
        var idParam = query["id"];

        if (action == "create")
        {
            SelectedTranslationId = null;
            await OpenTranslationPanelAsync();
        }
        else if (!string.IsNullOrEmpty(idParam) && Guid.TryParse(idParam, out var translationId))
        {
            SelectedTranslationId = translationId;
            var translation = AllTranslations.FirstOrDefault(t => t.Id == translationId);
            if (translation != null)
            {
                await OpenTranslationPanelAsync(translation);
            }
        }
        else
        {
            SelectedTranslationId = null;
            if (CurrentDialog != null)
            {
                await CurrentDialog.CloseAsync();
                CurrentDialog = null;
            }
        }
        StateHasChanged();
    }

    private async Task OpenTranslationPanelAsync(TranslationDto? translation = null)
    {
        var isEditMode = translation != null;
        var parameters = new TranslationPanelParameters
        {
            TranslationId = translation?.Id,
            DataSetId = DataSetId ?? translation?.DataSetId,
            OnDataChanged = async () =>
            {
                RefreshToken = Guid.NewGuid().ToString();
                LoadLayoutsAsync();
                await InvokeAsync(StateHasChanged);
            }
        };

        var newDialog = await DialogService.ShowPanelAsync<TranslationPanel>(parameters, new DialogParameters
        {
            Title = isEditMode ? "Edit Translation" : "Create New Translation",
            Width = "1200px",
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
        
        if(result.Cancelled && currentId == translation?.Id.ToString())
        {
            NavigationManager.NavigateTo(DataSetId != null ? "/translations/" + DataSetId : "/translations", false);
        }
        
        StateHasChanged();
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

    private async Task LoadAvailableCulturesAsync()
    {
        try
        {
            var query = new GetAvailableCulturesQuery();
            AvailableCultures = await RequestSender.SendAsync(query);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load available cultures");
        }
    }

    private async Task LoadFilterSettingsAsync()
    {
        try
        {
            var savedSettings = await LocalStorage.GetItemAsync<AppFilterSettings>(FilterStorageKey);
            if (savedSettings != null && savedSettings.Filters.Any())
            {
                FilterSettings = savedSettings;
            }
            else
            {
                // Initialize default filter settings
                FilterSettings = new AppFilterSettings
                {
                    Filters = new List<FilterSettings>
                    {
                        new FilterSettings { Id = "culture", Label = "Culture", Visible = true, OrderIndex = 0 },
                        new FilterSettings { Id = "notFilled", Label = "Not Filled", Visible = true, OrderIndex = 1 }
                    }
                };
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load filter settings");
            // Initialize default settings on error
            FilterSettings = new AppFilterSettings
            {
                Filters = new List<FilterSettings>
                {
                    new FilterSettings { Id = "culture", Label = "Culture", Visible = true, OrderIndex = 0 },
                    new FilterSettings { Id = "notFilled", Label = "Not Filled", Visible = true, OrderIndex = 1 }
                }
            };
        }
    }

    private async Task SaveFilterSettingsAsync()
    {
        try
        {
            await LocalStorage.SetItemAsync(FilterStorageKey, FilterSettings);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save filter settings");
        }
    }

    private async Task OpenFilterPanelAsync()
    {
        // Create a copy of filter settings to allow canceling changes
        // Using record 'with' expression to create shallow copies of each FilterSettings
        var filtersCopy = FilterSettings.Filters.Select(f => f with { }).ToList();

        var parameters = new FilterPanelParameters
        {
            Filters = filtersCopy
        };

        var dialog = await DialogService.ShowPanelAsync<FilterPanel>(parameters, new DialogParameters
        {
            Title = "Filter Settings",
            Width = "400px",
            TrapFocus = false,
            Modal = false,
            Id = $"filter-panel-{Guid.NewGuid()}"
        });

        var result = await dialog.Result;

        // Only apply changes if saved (not cancelled)
        if (!result.Cancelled && result.Data is List<FilterSettings> updatedFilters)
        {
            FilterSettings.Filters = updatedFilters;
            await SaveFilterSettingsAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private bool IsFilterVisible(string filterId)
    {
        var filter = FilterSettings.Filters.FirstOrDefault(f => f.Id == filterId);
        return filter?.Visible ?? false;
    }
}
