using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Modules.DataSets;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;

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

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    private List<DataSetDto> AllTranslationSets => AppContext.DataSets;
    private bool IsExporting { get; set; }

    private void OnDataSetFilterChanged(Guid? translationSetId)
    {
        var url = translationSetId.HasValue ? $"translations/{translationSetId}" : "translations";
        NavigationManager.NavigateTo(url);
    }

    private Appearance GetAppearanceForDataSet(Guid? translationSetId)
    {
        var isSelected = DataSetId == translationSetId;
        return isSelected ? Appearance.Accent : Appearance.Neutral;
    }

    private async Task OnExportToExcelAsync()
    {
        if (DataSetId == null)
        {
            return;
        }

        var parameters = new ExportTranslationsPanelParameters
        {
            DataSetId = DataSetId.Value
        };

        await DialogService.ShowPanelAsync<ExportTranslationsPanel>(parameters, new DialogParameters
        {
            Title = "Export Translations to Excel",
            Width = "400px",
            TrapFocus = false,
            Modal = false,
            Id = $"export-panel-{Guid.NewGuid()}"
        });
    }

    private async Task OnImportAsync()
    {
        if (DataSetId == null)
        {
            return;
        }

        var parameters = new ImportTranslationsPanelParameters
        {
            DataSetId = DataSetId.Value
        };

        await DialogService.ShowPanelAsync<ImportTranslationsPanel>(parameters, new DialogParameters
        {
            Title = "Import Translations from Excel",
            Width = "100%",
            TrapFocus = false,
            Modal = false,
            Id = $"import-panel-{Guid.NewGuid()}"
        });
    }

    private async Task OnRemoveDuplicatesAsync()
    {
        if (DataSetId == null)
        {
            return;
        }

        var parameters = new RemoveDuplicateTranslationsPanelParameters
        {
            DataSetId = DataSetId.Value
        };

        await DialogService.ShowPanelAsync<RemoveDuplicateTranslationsPanel>(parameters, new DialogParameters
        {
            Title = "Remove Duplicate Translations",
            Width = "500px",
            TrapFocus = false,
            Modal = false,
            Id = $"remove-duplicates-panel-{Guid.NewGuid()}"
        });
    }

    private async Task OnIndexTranslationsAsync()
    {
        if (DataSetId == null)
        {
            return;
        }

        // Show confirmation dialog
        var dialog = await DialogService.ShowConfirmationAsync(
            $"Are you sure you want to index translations in the selected dataset? This will:\n" +
            $"• Set InternalGroupName1 to 'Email' for translations starting with 'Email.'\n" +
            $"• Set InternalGroupName2 to 'EmailLayout' for translations that start with 'Email.' and also contain 'Layout'.",
            "Yes",
            "No",
            "Confirm Indexing");

        var result = await dialog.Result;

        if (result.Cancelled)
        {
            return;
        }

        try
        {
            var command = new IndexTranslationsCommand
            {
                DataSetId = DataSetId.Value
            };

            var indexResult = await RequestSender.SendAsync(command);

            if (indexResult.Errors?.Any() == true)
            {
                ToastService.ShowWarning($"Indexing completed with warnings: {indexResult.UpdatedCount} translations updated. {indexResult.Errors.Count} error(s) encountered.");
            }
            else
            {
                ToastService.ShowSuccess($"Successfully indexed {indexResult.ProcessedCount} translations. {indexResult.UpdatedCount} translations updated.");
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Failed to index translations: {ex.Message}");
        }
    }
}
