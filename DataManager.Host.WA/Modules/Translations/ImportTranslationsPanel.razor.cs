using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using DataManager.Host.WA.Modules.Translations.Models;
using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Host.WA.Services;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace DataManager.Host.WA.Modules.Translations;

public partial class ImportTranslationsPanel : IDialogContentComponent<ImportTranslationsPanelParameters>
{
    [Parameter]
    public ImportTranslationsPanelParameters Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }

    [Inject]
    private IRequestSender RequestSender { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    [Inject]
    private ILogger<ImportTranslationsPanel> Logger { get; set; } = null!;

    private bool IsLoadingFile { get; set; }
    private bool IsImporting { get; set; }
    private string? ErrorMessage { get; set; }
    private DataTable? ExcelDataTable { get; set; }
    private List<string> TargetColumns = new() { "InternalGroupName1", "InternalGroupName2", "ResourceName", "TranslationName", "CultureName", "Content" };
    private Dictionary<string, string> ColumnMappings = new();
    private const long MaxFileSize = 50 * 1024 * 1024; // 50 MB

    private IEnumerable<DataRow>? ExcelDataRows => ExcelDataTable?.Rows.Cast<DataRow>();

    private async Task OnFileChanged(InputFileChangeEventArgs e)
    {
        IsLoadingFile = true;
        ErrorMessage = null;
        ExcelDataTable = null;
        ColumnMappings = new();
        StateHasChanged();

        try
        {
            var file = e.File;
            if (file != null)
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var memoryStream = new MemoryStream())
                {
                    await file.OpenReadStream(MaxFileSize).CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    using (var reader = ExcelReaderFactory.CreateReader(memoryStream))
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        });
                        ExcelDataTable = result.Tables[0];
                        foreach (DataColumn col in ExcelDataTable.Columns)
                        {
                            ColumnMappings.Add(col.ColumnName, TargetColumns.FirstOrDefault(t => t.Equals(col.ColumnName, StringComparison.OrdinalIgnoreCase)) ?? "");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred while reading the file: {ex.Message}";
            Logger.LogError(ex, "Error loading Excel file");
        }
        finally
        {
            IsLoadingFile = false;
            StateHasChanged();
        }
    }

    private async Task HandleSubmitAsync()
    {
        if (ExcelDataTable == null)
        {
            return;
        }

        var importedTranslations = ExcelDataTable.Rows.Cast<DataRow>().Select(row =>
        {
            var dto = new ImportedTranslationDto { OriginalRow = row };
            foreach (var mapping in ColumnMappings)
            {
                if (!string.IsNullOrEmpty(mapping.Value) && ExcelDataTable.Columns.Contains(mapping.Key))
                {
                    var cellValue = row[mapping.Key]?.ToString();
                    var property = typeof(ImportedTranslationDto).GetProperty(mapping.Value);
                    if (property != null && !string.IsNullOrEmpty(cellValue))
                    {
                        property.SetValue(dto, cellValue);
                    }
                }
            }
            return dto;
        }).ToList();
        
        if (importedTranslations == null || !importedTranslations.Any())
        {
            ToastService.ShowError("No translations found in the file.");
            return;
        }

        var translationsToImport = importedTranslations.Where(t => t.ShouldImport).ToList();
        if (!translationsToImport.Any())
        {
            ToastService.ShowWarning("No translations selected for import.");
            return;
        }

        IsImporting = true;
        ErrorMessage = null;
        StateHasChanged();

        try
        {
            // Prepare the import command
            var importDtos = translationsToImport.Select(t => new ImportTranslationInput
            {
                ResourceName = t.ResourceName,
                TranslationName = t.TranslationName,
                Content = t.Content,
                CultureName = t.CultureName,
            }).ToList();

            var command = new ImportTranslationsCommand
            {
                DataSetId = Content.DataSetId,
                Translations = importDtos
            };

            var result = await RequestSender.SendAsync(command);

            if (result.FailedCount > 0)
            {
                ToastService.ShowWarning($"Import completed with errors: {result.ImportedCount} succeeded, {result.FailedCount} failed.");
                Logger.LogWarning("Import completed with {FailedCount} failures", result.FailedCount);
                foreach (var error in result.Errors)
                {
                    Logger.LogDebug("Import error: {Error}", error);
                }
            }
            else
            {
                ToastService.ShowSuccess($"Successfully imported {result.ImportedCount} translations.");
            }

            // Close the panel after successful import
            await Dialog!.CloseAsync(DialogResult.Ok(true));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Import failed: {ex.Message}";
            ToastService.ShowError(ErrorMessage);
            Logger.LogError(ex, "Error importing translations");
        }
        finally
        {
            IsImporting = false;
            StateHasChanged();
        }
    }

    private Task HandleResetAsync()
    {
        ExcelDataTable = null;
        ColumnMappings = new();
        ErrorMessage = null;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task HandleCancelAsync()
    {
        await Dialog!.CancelAsync();
    }
}

public class ImportTranslationsPanelParameters
{
    public Guid DataSetId { get; set; }
}
