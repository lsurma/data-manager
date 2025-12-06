using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.DataSet;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace DataManager.Host.WA.Modules.Translations;

public partial class TranslationsPage : ComponentBase
{
    [Parameter]
    public Guid? DataSetId { get; set; }

    [CascadingParameter]
    public AppDataContext? CascadingAppContext { get; set; }

    [Inject]
    private AppDataContext InjectedAppContext { get; set; } = null!;

    /// <summary>
    /// Gets the AppDataContext from cascading parameter if available, otherwise uses injected service
    /// </summary>
    private AppDataContext AppContext => CascadingAppContext ?? InjectedAppContext;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private NavigationHelper NavHelper { get; set; } = null!;

    [Inject]
    private IRequestSender RequestSender { get; set; } = null!;

    private List<DataSetDto> AllDataSets => AppContext.DataSets;

    private void OnDataSetFilterChanged(Guid? dataSetId)
    {
        var url = dataSetId.HasValue ? $"translations/{dataSetId}" : "translations";
        NavigationManager.NavigateTo(url);
    }

    private Appearance GetAppearanceForDataSet(Guid? dataSetId)
    {
        var isSelected = DataSetId == dataSetId;
        return isSelected ? Appearance.Accent : Appearance.Neutral;
    }
}
