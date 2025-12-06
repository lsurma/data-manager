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
    public Guid? DataSetId { get; set; }
    
    [Parameter]
    public RenderFragment ToolbarTemplate { get; set; } = null!;

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

    private List<TranslationDto> AllTranslations { get; set; } = new();
    private List<DataSetDto> AllDataSets => AppContext.DataSets;
    private List<TranslationDto> AllLayouts { get; set; } = new();
    private IDialogReference? CurrentDialog;
    private string? RefreshToken { get; set; }
    private IList<TranslationDto> SelectedRows = new List<TranslationDto>();
    private GetTranslationsQuery _currentQuery = new GetTranslationsQuery
    {
        Pagination = new PaginationParameters { PageNumber = 1, PageSize = 15 }
    };

    private int TotalItems;
    private int PageSize = 20;
    private string? SearchTerm;
    private string? CultureNameFilter;
    private Guid? SelectedTranslationId;

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
                Pagination = new PaginationParameters { Skip = 0, PageSize = PageSize }
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
        RefreshToken = Guid.NewGuid().ToString();
    }

    private void OnSearchChanged()
    {
        _currentQuery = new GetTranslationsQuery
        {
            Filtering = new FilteringParameters
            {
                QueryFilters = BuildQueryFilters()
            },
            Pagination = new PaginationParameters { Skip = 0, PageSize = PageSize }
        };
        RefreshToken = Guid.NewGuid().ToString();
    }

    private void OnCultureFilterChanged()
    {
        _currentQuery = new GetTranslationsQuery
        {
            Filtering = new FilteringParameters
            {
                QueryFilters = BuildQueryFilters()
            },
            Pagination = new PaginationParameters { Skip = 0, PageSize = PageSize }
        };
        RefreshToken = Guid.NewGuid().ToString();
    }

    private List<IQueryFilter> BuildQueryFilters()
    {
        var filters = new List<IQueryFilter>();
        
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
            DataSetId = DataSetId,
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
}
