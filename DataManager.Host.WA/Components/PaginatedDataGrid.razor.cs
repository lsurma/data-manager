using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace DataManager.Host.WA.Components;

public partial class PaginatedDataGrid<TItem> : ComponentBase
{
    private RadzenDataGrid<TItem> _dataGrid = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = null!;

    [Parameter]
    public List<TItem> Items { get; set; } = new();

    [Parameter]
    public int TotalItems { get; set; }

    [Parameter]
    public int PageSize { get; set; } = 20;

    [Parameter]
    public string SearchPlaceholder { get; set; } = "Search...";

    [Parameter]
    public string? SearchTerm { get; set; }

    [Parameter]
    public EventCallback<string?> SearchTermChanged { get; set; }

    [Parameter]
    public EventCallback<LoadDataArgs> LoadData { get; set; }

    [Parameter]
    public EventCallback OnSearchChanged { get; set; }

    [Parameter]
    public IList<TItem>? SelectedRows { get; set; }

    [Parameter]
    public EventCallback<IList<TItem>> SelectedRowsChanged { get; set; }

    [Parameter]
    public RenderFragment? Columns { get; set; }

    [Parameter]
    public RenderFragment? ActionsTemplate { get; set; }

    [Parameter]
    public RenderFragment? FiltersTemplate { get; set; }

    [Parameter]
    public bool AllowFiltering { get; set; } = true;

    [Parameter]
    public bool AllowSorting { get; set; } = true;

    [Parameter]
    public bool AllowPaging { get; set; } = true;

    [Parameter]
    public DataGridSelectionMode SelectionMode { get; set; } = DataGridSelectionMode.Single;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        await LoadSettingsAsync();
    }

    private async Task OnLoadData(LoadDataArgs args)
    {
        if (LoadData.HasDelegate)
        {
            await LoadData.InvokeAsync(args);
        }
    }

    private async Task OnSelectionChanged(IList<TItem> selectedRows)
    {
        SelectedRows = selectedRows;

        if (SelectedRowsChanged.HasDelegate)
        {
            await SelectedRowsChanged.InvokeAsync(selectedRows);
        }
    }

    private void HandleSearchChanged()
    {
        if (SearchTermChanged.HasDelegate)
        {
            _ = SearchTermChanged.InvokeAsync(SearchTerm);
        }

        if (OnSearchChanged.HasDelegate)
        {
            _ = OnSearchChanged.InvokeAsync();
        }
    }

    private async Task HandleClearSearch()
    {
        SearchTerm = null;

        if (SearchTermChanged.HasDelegate)
        {
            await SearchTermChanged.InvokeAsync(SearchTerm);
        }

        if (OnSearchChanged.HasDelegate)
        {
            await OnSearchChanged.InvokeAsync();
        }
    }
}