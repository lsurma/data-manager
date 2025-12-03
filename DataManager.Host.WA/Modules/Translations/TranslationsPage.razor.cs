using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.DataSet;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace DataManager.Host.WA.Modules.Translations;

public partial class TranslationsPage : ComponentBase, IDisposable
{
    [Parameter]
    public string? DataSetId { get; set; }

    [CascadingParameter(Name = "AppDataContext")]
    public AppDataContext AppContext { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private NavigationHelper NavHelper { get; set; } = null!;

    [Inject]
    private IRequestSender RequestSender { get; set; } = null!;

    private List<DataSetDto> AllDataSets => AppContext?.DataSets ?? new List<DataSetDto>();
    private List<IQueryFilter> _filters = new();

    protected override void OnInitialized()
    {
        BuildFilters();
        
        // Subscribe to context refresh events
        if (AppContext != null)
        {
            AppContext.OnDataRefreshed += HandleContextRefreshed;
        }
    }

    protected override void OnParametersSet()
    {
        BuildFilters();
    }

    private void HandleContextRefreshed()
    {
        StateHasChanged();
    }

    private void BuildFilters()
    {
        var filters = new List<IQueryFilter>();
        if (Guid.TryParse(DataSetId, out var dataSetId))
        {
            filters.Add(new DataSetIdFilter { Value = dataSetId });
        }
        _filters = filters;
    }

    private void OnDataSetFilterChanged(Guid? dataSetId)
    {
        var url = dataSetId.HasValue ? $"translations/{dataSetId}" : "translations";
        NavigationManager.NavigateTo(url);
    }

    private Appearance GetAppearanceForDataSet(Guid? dataSetId)
    {
        var isSelected = (DataSetId == dataSetId?.ToString()) || (DataSetId == null && !dataSetId.HasValue);
        return isSelected ? Appearance.Accent : Appearance.Neutral;
    }

    public void Dispose()
    {
        // Unsubscribe from context refresh events
        if (AppContext != null)
        {
            AppContext.OnDataRefreshed -= HandleContextRefreshed;
        }
    }
}
