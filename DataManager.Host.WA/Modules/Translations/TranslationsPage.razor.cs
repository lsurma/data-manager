using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.TranslationsSet;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace DataManager.Host.WA.Modules.Translations;

public partial class TranslationsPage : ComponentBase
{
    [Parameter]
    public Guid? TranslationsSetId { get; set; }

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

    private List<TranslationsSetDto> AllTranslationsSets => AppContext.TranslationsSets;

    private void OnDataSetFilterChanged(Guid? translationsSetId)
    {
        var url = translationsSetId.HasValue ? $"translations/{translationsSetId}" : "translations";
        NavigationManager.NavigateTo(url);
    }

    private Appearance GetAppearanceForDataSet(Guid? translationsSetId)
    {
        var isSelected = TranslationsSetId == translationsSetId;
        return isSelected ? Appearance.Accent : Appearance.Neutral;
    }
}
