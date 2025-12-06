using System.Web;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.TranslationsSet;
using DataManager.Host.WA.Components;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;

namespace DataManager.Host.WA.Modules.TranslationsSets;

public partial class TranslationsSetsPage : ComponentBase, IDisposable
{
    [CascadingParameter]
    public AppDataContext? CascadingAppContext { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; } = null!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;
    
    [Inject]
    private NavigationHelper NavHelper { get; set; } = null!;
    
    private List<TranslationsSetDto> AllTranslationsSets { get; set; } = new();
    private IDialogReference? CurrentDialog { get; set; }
    private string RefreshToken { get; set; } = Guid.NewGuid().ToString();
    private IList<TranslationsSetDto> SelectedRows { get; set; } = new List<TranslationsSetDto>();
    
    private int PageSize { get; set; } = 15;
    
    protected int TotalItems { get; set; }
    
    private string? SearchTerm { get; set; }
    private Guid? SelectedTranslationsSetId { get; set; }
    
    private GetTranslationsSetsQuery CurrentQuery { get; set; } = new GetTranslationsSetsQuery
    {
        Pagination = new PaginationParameters { PageNumber = 1, PageSize = 15 }
    };

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
    }
    
    private void HandleDataFetched(DataFetchedEventArgs<PaginatedList<TranslationsSetDto>> eventArgs)
    {
        AllTranslationsSets = eventArgs.Data.Items;
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
    
    private Task OnDataGridSelectionChanged(IList<TranslationsSetDto> selectedRows)
    {
        SelectedRows = selectedRows;
        
        if (selectedRows != null && selectedRows.Count > 0)
        {
            var translationsSet = selectedRows[0];
            SelectedTranslationsSetId = translationsSet.Id;

            NavigationManager.NavigateTo($"/datasets?id={translationsSet.Id}", false);
        }
        else
        {
            SelectedTranslationsSetId = null;
        }
        
        return Task.CompletedTask;
    }
    
    private void RestoreDataGridSelection()
    {
        if (!SelectedTranslationsSetId.HasValue)
        {
            SelectedRows = new List<TranslationsSetDto>();
            return;
        }
        
        // Try to find in current grid page first, fallback to AllTranslationsSets if not found
        var selectedTranslationsSet = AllTranslationsSets.FirstOrDefault(i => i.Id == SelectedTranslationsSetId.Value);
        
        if (selectedTranslationsSet != null)
        {
            SelectedRows = new List<TranslationsSetDto> { selectedTranslationsSet };
        }
        else
        {
            SelectedRows = new List<TranslationsSetDto>();
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
            CurrentQuery = new GetTranslationsSetsQuery
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
        CurrentQuery = new GetTranslationsSetsQuery
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
                SelectedTranslationsSetId = null;
                await OpenTranslationsSetPanelAsync();
            }
            else if (!string.IsNullOrEmpty(idParam) && Guid.TryParse(idParam, out var translationsSetId))
            {
                SelectedTranslationsSetId = translationsSetId;
                var translationsSet = AllTranslationsSets.FirstOrDefault(i => i.Id == translationsSetId);
                
                if (translationsSet != null)
                {
                    await OpenTranslationsSetPanelAsync(translationsSet);
                }
            }
            else
            {
                SelectedTranslationsSetId = null;
                
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
    
    private async Task OpenTranslationsSetPanelAsync(TranslationsSetDto? translationsSet = null)
    {
        var isEditMode = translationsSet != null;

        // Use TranslationsSets from AppContext
        var availableTranslationsSets = CascadingAppContext?.TranslationsSets;

        var parameters = new TranslationsSetPanelParameters
        {
            TranslationsSet = isEditMode 
                ? translationsSet! with { }
                : new TranslationsSetDto
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTimeOffset.UtcNow
                },

            IsEditMode = isEditMode,
            
            AvailableTranslationsSets = isEditMode 
                ? availableTranslationsSets?.Where(i => i.Id != translationsSet!.Id).ToList() ?? []
                : availableTranslationsSets ?? [],
            
            OnDataChanged = async () =>
            {
                RefreshToken = Guid.NewGuid().ToString();

                // Refresh context to get updated TranslationsSets
                if (CascadingAppContext != null)
                {
                    await CascadingAppContext.RefreshAsync();
                }

                await InvokeAsync(StateHasChanged);
            }
        };

        var newDialog = await DialogService.ShowPanelAsync<TranslationsSetPanel>(parameters, new DialogParameters
        {
            Title = isEditMode ? "Edit TranslationsSet" : "Create New TranslationsSet",
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
        
        if(result.Cancelled && currentId == translationsSet?.Id.ToString())
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
