using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.TranslationsSet;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;

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

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    private List<TranslationsSetDto> AllTranslationsSets => AppContext.TranslationsSets;
    private bool IsExporting { get; set; }

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

    private async Task OnExportToExcelAsync()
    {
        if (TranslationsSetId == null)
        {
            return;
        }

        try
        {
            IsExporting = true;
            StateHasChanged();

            var query = new ExportTranslationsQuery
            {
                Format = "xlsx",
                Filtering = new FilteringParameters
                {
                    QueryFilters = new List<IQueryFilter>
                    {
                        new TranslationsSetIdFilter { Value = TranslationsSetId.Value }
                    }
                }
            };

            var downloadedFile = await RequestSender.DownloadFileAsync(query);

            await JSRuntime.InvokeVoidAsync("downloadFile", downloadedFile.FileName, downloadedFile.ContentType, downloadedFile.Content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Export failed: {ex.Message}");
        }
        finally
        {
            IsExporting = false;
            StateHasChanged();
        }
    }
}
