using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DataManager.Host.WA.Modules.Translations;

public partial class ExportTranslationsPanel : IDialogContentComponent<ExportTranslationsPanelParameters>
{
    [Parameter]
    public ExportTranslationsPanelParameters Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }

    [CascadingParameter]
    public AppDataContext? CascadingAppDataContext { get; set; }

    [Inject]
    private IRequestSender RequestSender { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    private bool IsExporting { get; set; }
    private bool IsLoading { get; set; }
    private string? ErrorMessage { get; set; }

    private List<string> AvailableCultures { get; set; } = new();
    private ExportModel Model { get; set; } = new();

    protected override void OnInitialized()
    {
        LoadDataAsync();
        StateHasChanged();
    }

    private void LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Get the DataSet from AppDataContext
            var dataSet = CascadingAppDataContext?.DataSets
                .FirstOrDefault(x => x.Id == Content.DataSetId);

            if (dataSet == null)
            {
                ErrorMessage = "DataSet not found.";
                return;
            }

            // Get available cultures
            if (dataSet.AvailableCultures != null && dataSet.AvailableCultures.Any())
            {
                AvailableCultures = dataSet.AvailableCultures.OrderBy(c => c).ToList();
            }

            if (AvailableCultures.Any())
            {
                // Set default base culture to "en-US" if it exists, otherwise first culture
                var defaultBaseCulture = AvailableCultures.Contains("en-US") 
                    ? "en-US" 
                    : AvailableCultures.First();

                Model = new ExportModel
                {
                    BaseCulture = defaultBaseCulture,
                    TargetCulture = AvailableCultures.First()
                };
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load export options: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task HandleSubmitAsync()
    {
        if (Model == null || string.IsNullOrEmpty(Model.BaseCulture) || string.IsNullOrEmpty(Model.TargetCulture))
        {
            ErrorMessage = "Please select both base and target cultures.";
            return;
        }

        try
        {
            IsExporting = true;
            ErrorMessage = null;
            StateHasChanged();

            var query = new ExportTranslationsQuery
            {
                Format = "xlsx",
                BaseCulture = Model.BaseCulture,
                TargetCulture = Model.TargetCulture,
                DataSetId = Content.DataSetId,
            };

            var downloadedFile = await RequestSender.DownloadFileAsync(query);
            var filename = string.IsNullOrWhiteSpace(downloadedFile.FileName) 
                ? "translations.xlsx" 
                : downloadedFile.FileName;

            await JSRuntime.InvokeVoidAsync("downloadFile", filename, downloadedFile.ContentType, downloadedFile.Content);

            ToastService.ShowSuccess($"Successfully exported translations (Base: {Model.BaseCulture}, Target: {Model.TargetCulture})");

            await Dialog!.CloseAsync(DialogResult.Ok(true));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to export translations: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
            StateHasChanged();
        }
    }

    private async Task HandleCancelAsync()
    {
        await Dialog!.CancelAsync();
    }

    private class ExportModel
    {
        public string BaseCulture { get; set; } = string.Empty;
        public string TargetCulture { get; set; } = string.Empty;
    }
}

public class ExportTranslationsPanelParameters
{
    public Guid DataSetId { get; set; }
}
