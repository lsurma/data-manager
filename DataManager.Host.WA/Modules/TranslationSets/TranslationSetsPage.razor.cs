using System.Web;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.TranslationSet;
using DataManager.Host.WA.Components;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;

namespace DataManager.Host.WA.Modules.TranslationSets;

public partial class TranslationSetsPage : ComponentBase, IDisposable
{
    [CascadingParameter]
    public AppDataContext? CascadingAppContext { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; } = null!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;
    
    [Inject]
    private NavigationHelper NavHelper { get; set; } = null!;
    
    private List<TranslationSetDto> AllTranslationSets { get; set; } = new();
    private IDialogReference? CurrentDialog { get; set; }
    private string RefreshToken { get; set; } = Guid.NewGuid().ToString();
    private IList<TranslationSetDto> SelectedRows { get; set; } = new List<TranslationSetDto>();
    
    private int PageSize { get; set; } = 15;
    
    protected int TotalItems { get; set; }
    
    private string? SearchTerm { get; set; }
    private Guid? SelectedTranslationSetId { get; set; }
    
    private GetTranslationSetsQuery CurrentQuery { get; set; } = new GetTranslationSetsQuery
    {
        Pagination = new PaginationParameters { PageNumber = 1, PageSize = 15 }
    };

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
    }
    
    private void HandleDataFetched(DataFetchedEventArgs<PaginatedList<TranslationSetDto>> eventArgs)
    {
        AllTranslationSets = eventArgs.Data.Items;
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
    
    private Task OnDataGridSelectionChanged(IList<TranslationSetDto> selectedRows)
    {
        SelectedRows = selectedRows;
        
        if (selectedRows != null && selectedRows.Count > 0)
        {
            var translationSet = selectedRows[0];
            SelectedTranslationSetId = translationSet.Id;

            NavigationManager.NavigateTo($"/datasets?id={translationSet.Id}", false);
        }
        else
        {
            SelectedTranslationSetId = null;
        }
        
        return Task.CompletedTask;
    }
    
    private void RestoreDataGridSelection()
    {
        if (!SelectedTranslationSetId.HasValue)
        {
            SelectedRows = new List<TranslationSetDto>();
            return;
        }
        
        // Try to find in current grid page first, fallback to AllTranslationSets if not found
        var selectedDataSet = AllTranslationSets.FirstOrDefault(i => i.Id == SelectedTranslationSetId.Value);
        
        if (selectedDataSet != null)
        {
            SelectedRows = new List<TranslationSetDto> { selectedDataSet };
        }
        else
        {
            SelectedRows = new List<TranslationSetDto>();
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
            CurrentQuery = new GetTranslationSetsQuery
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
        CurrentQuery = new GetTranslationSetsQuery
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
                SelectedTranslationSetId = null;
                await OpenTranslationSetPanelAsync();
            }
            else if (!string.IsNullOrEmpty(idParam) && Guid.TryParse(idParam, out var translationSetId))
            {
                SelectedTranslationSetId = translationSetId;
                var translationSet = AllTranslationSets.FirstOrDefault(i => i.Id == translationSetId);
                
                if (translationSet != null)
                {
                    await OpenTranslationSetPanelAsync(translationSet);
                }
            }
            else
            {
                SelectedTranslationSetId = null;
                
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
    
    private async Task OpenTranslationSetPanelAsync(TranslationSetDto? translationSet = null)
    {
        var isEditMode = translationSet != null;

        // Use TranslationSets from AppContext
        var availableTranslationSets = CascadingAppContext?.TranslationSets;

        var parameters = new TranslationSetPanelParameters
        {
            TranslationSet = isEditMode 
                ? translationSet! with { }
                : new TranslationSetDto
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTimeOffset.UtcNow
                },

            IsEditMode = isEditMode,
            
            AvailableTranslationSets = isEditMode 
                ? availableTranslationSets?.Where(i => i.Id != translationSet!.Id).ToList() ?? []
                : availableTranslationSets ?? [],
            
            OnDataChanged = async () =>
            {
                RefreshToken = Guid.NewGuid().ToString();

                // Refresh context to get updated TranslationSets
                if (CascadingAppContext != null)
                {
                    await CascadingAppContext.RefreshAsync();
                }

                await InvokeAsync(StateHasChanged);
            }
        };

        var newDialog = await DialogService.ShowPanelAsync<TranslationSetPanel>(parameters, new DialogParameters
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
        
        if(result.Cancelled && currentId == translationSet?.Id.ToString())
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
