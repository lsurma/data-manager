using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Modules.DataSets;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace DataManager.Host.WA.Modules.Translations;

public partial class RemoveDuplicateTranslationsPanel : IDialogContentComponent<RemoveDuplicateTranslationsPanelParameters>
{
    [Parameter]
    public RemoveDuplicateTranslationsPanelParameters Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }

    [CascadingParameter]
    public AppDataContext? CascadingAppDataContext { get; set; }

    [Inject]
    private IRequestSender RequestSender { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    private bool IsProcessing { get; set; }
    private bool IsLoading { get; set; }
    private bool IsCompleted { get; set; }
    private string? ErrorMessage { get; set; }

    private List<DataSetDto> AvailableDataSets { get; set; } = new();
    private string CurrentDataSetName { get; set; } = string.Empty;
    private RemoveDuplicateTranslationsModel Model { get; set; } = new();
    private RemoveDuplicateTranslationsResult? Result { get; set; }

    protected override void OnInitialized()
    {
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Get all datasets from AppDataContext
            var allDataSets = CascadingAppDataContext?.DataSets ?? new List<DataSetDto>();

            // Find the current dataset
            var currentDataSet = allDataSets.FirstOrDefault(x => x.Id == Content.DataSetId);
            CurrentDataSetName = currentDataSet?.Name ?? "Unknown";

            // Filter out the current dataset from available base datasets
            AvailableDataSets = allDataSets
                .Where(d => d.Id != Content.DataSetId)
                .OrderBy(d => d.Name)
                .ToList();

            if (!AvailableDataSets.Any())
            {
                ErrorMessage = "No other datasets available to use as base dataset.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load datasets: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task HandleSubmitAsync()
    {
        if (string.IsNullOrEmpty(Model.BaseDataSetId))
        {
            ErrorMessage = "Please select a base dataset.";
            return;
        }

        if (!Guid.TryParse(Model.BaseDataSetId, out var baseDataSetGuid))
        {
            ErrorMessage = "Invalid base dataset ID.";
            return;
        }

        try
        {
            IsProcessing = true;
            ErrorMessage = null;
            StateHasChanged();

            var command = new RemoveDuplicateTranslationsCommand
            {
                SpecificDataSetId = Content.DataSetId,
                BaseDataSetId = baseDataSetGuid
            };

            Result = await RequestSender.SendAsync(command);

            IsCompleted = true;

            if (Result.Errors?.Any() == true)
            {
                ToastService.ShowWarning($"Completed with warnings: {Result.RemovedCount} duplicates removed, {Result.Errors.Count} error(s) encountered.");
            }
            else
            {
                ToastService.ShowSuccess($"Successfully removed {Result.RemovedCount} duplicate translations.");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to remove duplicates: {ex.Message}";
            ToastService.ShowError($"Failed to remove duplicates: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
            StateHasChanged();
        }
    }

    private async Task HandleCancelAsync()
    {
        if (IsCompleted)
        {
            await Dialog!.CloseAsync(DialogResult.Ok(true));
        }
        else
        {
            await Dialog!.CancelAsync();
        }
    }

    private class RemoveDuplicateTranslationsModel
    {
        public string? BaseDataSetId { get; set; }
    }
}

public class RemoveDuplicateTranslationsPanelParameters
{
    public Guid DataSetId { get; set; }
}
