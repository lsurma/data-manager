using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using DataManager.Host.WA.Modules.Translations.Models;
using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Contracts.Modules.DataSet;
using DataManager.Host.WA.Services;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace DataManager.Host.WA.Modules.Translations
{
    public partial class ImportTranslationsPage : ComponentBase, IDisposable
    {
        [CascadingParameter]
        public AppDataContext? CascadingAppContext { get; set; }

        [Inject]
        private AppDataContext InjectedAppContext { get; set; } = null!;

        /// <summary>
        /// Gets the AppDataContext from cascading parameter if available, otherwise uses injected service
        /// </summary>
        private AppDataContext AppContext => CascadingAppContext ?? InjectedAppContext;

        [Inject]
        private IRequestSender RequestSender { get; set; } = null!;

        [Inject]
        private IToastService ToastService { get; set; } = null!;

        [Inject]
        private ILogger<ImportTranslationsPage> Logger { get; set; } = null!;

        private List<ImportedTranslationDto>? ImportedTranslations;
        private bool IsLoadingFile;
        private bool IsImporting;
        private string? ErrorMessage;
        private DataTable? ExcelDataTable;
        private List<string> TargetColumns = new() { "InternalGroupName1", "InternalGroupName2", "ResourceName", "TranslationName", "CultureName", "Content" };
        private Dictionary<string, string> ColumnMappings = new();
        private List<DataSetDto> AvailableDataSets => AppContext.DataSets;
        private Guid? SelectedDataSetId;
        private DataSetDto? SelectedDataSet;
        private string? SelectedDataSetValue
        {
            get => SelectedDataSetId?.ToString();
            set
            {
                if (Guid.TryParse(value, out var id))
                {
                    SelectedDataSetId = id;
                }
                else if (!string.IsNullOrEmpty(value))
                {
                    // Log invalid GUID value for debugging
                    Logger.LogWarning("Invalid GUID value for dataset selection: {Value}", value);
                }
            }
        }

        private static readonly Regex ErrorMessageRegex = 
            new Regex(
                @"Failed to import translation '([^']+)' \(([^)]+)\):",
                RegexOptions.Compiled);

        private IEnumerable<DataRow>? ExcelDataRows => ExcelDataTable?.Rows.Cast<DataRow>();

        protected override void OnInitialized()
        {
            // Initialize selected dataset from context
            if (AvailableDataSets.Any())
            {
                SelectedDataSetId = AvailableDataSets.FirstOrDefault()?.Id;
            }
            
            // Subscribe to context refresh events
            if (AppContext != null)
            {
                AppContext.OnDataRefreshed += HandleContextRefreshed;
            }
        }

        private void HandleContextRefreshed()
        {
            // Re-select first dataset if current one is not available anymore
            if (SelectedDataSetId != null && !AvailableDataSets.Any(ds => ds.Id == SelectedDataSetId))
            {
                SelectedDataSetId = AvailableDataSets.FirstOrDefault()?.Id;
            }
            
            StateHasChanged();
        }

        private async Task OnFileChanged(InputFileChangeEventArgs e)
        {
            IsLoadingFile = true;
            ErrorMessage = null;
            ImportedTranslations = null;
            ExcelDataTable = null;
            ColumnMappings = new();

            try
            {
                var file = e.File;
                if (file != null)
                {
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                    using (var memoryStream = new MemoryStream())
                    {
                        await file.OpenReadStream(long.MaxValue).CopyToAsync(memoryStream);
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
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsLoadingFile = false;
            }
        }

        private async Task StartImportAsync()
        {
             if (ExcelDataTable == null) return;

            ImportedTranslations = ExcelDataTable.Rows.Cast<DataRow>().Select(row =>
            {
                var dto = new ImportedTranslationDto { OriginalRow = row };
                foreach (var mapping in ColumnMappings)
                {
                    if (!string.IsNullOrEmpty(mapping.Value) && ExcelDataTable.Columns.Contains(mapping.Key))
                    {
                        var cellValue = row[mapping.Key].ToString();
                        var property = typeof(ImportedTranslationDto).GetProperty(mapping.Value);
                        if (property != null && !string.IsNullOrEmpty(cellValue))
                        {
                            property.SetValue(dto, cellValue);
                        }
                    }
                }
                return dto;
            }).ToList();
            
            if (ImportedTranslations == null || !ImportedTranslations.Any() || !SelectedDataSetId.HasValue)
            {
                ToastService.ShowError("Please select a data set and map columns before importing.");
                return;
            }

            var translationsToImport = ImportedTranslations.Where(t => t.ShouldImport).ToList();
            if (!translationsToImport.Any())
            {
                ToastService.ShowWarning("No translations selected for import.");
                return;
            }

            IsImporting = true;
            ErrorMessage = null;

            try
            {
                // Set all selected translations to InProgress
                foreach (var translation in translationsToImport)
                {
                    translation.Status = ImportStatus.InProgress;
                }
                StateHasChanged();

                // Prepare the import command
                var importDtos = translationsToImport.Select(t => new ImportTranslationDto
                {
                    ResourceName = t.ResourceName,
                    TranslationName = t.TranslationName,
                    Content = t.Content,
                    CultureName = t.CultureName,
                    InternalGroupName1 = t.InternalGroupName1,
                    InternalGroupName2 = t.InternalGroupName2,
                    ContentTemplate = t.ContentTemplate
                }).ToList();

                var command = new ImportTranslationsCommand
                {
                    DataSetId = SelectedDataSetId.Value,
                    Translations = importDtos
                };

                var result = await RequestSender.SendAsync(command);

                // Mark all as success initially
                foreach (var translation in translationsToImport)
                {
                    translation.Status = ImportStatus.Success;
                    translation.StatusMessage = "Imported successfully";
                }

                // Update failed translations based on error messages
                // Error format: "Failed to import translation '{TranslationName}' ({ResourceName}): {message}"
                var unmatchedErrors = new List<string>();
                var matchedErrorCount = 0;
                
                foreach (var error in result.Errors)
                {
                    // Try to extract translation name and resource name from error message
                    var match = ErrorMessageRegex.Match(error);
                    
                    if (match.Success)
                    {
                        var translationName = match.Groups[1].Value;
                        var resourceName = match.Groups[2].Value;
                        
                        var failedTranslation = translationsToImport.FirstOrDefault(t => 
                            t.TranslationName == translationName && t.ResourceName == resourceName);
                        
                        if (failedTranslation != null)
                        {
                            failedTranslation.Status = ImportStatus.Failed;
                            failedTranslation.StatusMessage = error;
                            matchedErrorCount++;
                        }
                        else
                        {
                            unmatchedErrors.Add(error);
                        }
                    }
                    else
                    {
                        unmatchedErrors.Add(error);
                    }
                }

                // Log any unmatched errors for debugging
                if (unmatchedErrors.Any())
                {
                    Logger.LogWarning("Could not match {Count} error(s) to translations during import", unmatchedErrors.Count);
                    foreach (var error in unmatchedErrors)
                    {
                        Logger.LogDebug("Unmatched error: {Error}", error);
                    }
                }

                // Handle case where we have more failures than matched errors
                if (result.FailedCount > matchedErrorCount)
                {
                    // Some errors might not have been properly formatted or matched
                    var unmatchedFailures = result.FailedCount - matchedErrorCount;
                    ErrorMessage = $"Warning: {unmatchedFailures} translation(s) failed but could not be matched to specific items.";
                    Logger.LogWarning("Import had {UnmatchedFailures} unmatched failures", unmatchedFailures);
                }

                ToastService.ShowSuccess($"Import completed: {result.ImportedCount} succeeded, {result.FailedCount} failed.");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Import failed: {ex.Message}";
                ToastService.ShowError(ErrorMessage);

                // Mark all in-progress as failed
                foreach (var translation in translationsToImport.Where(t => t.Status == ImportStatus.InProgress))
                {
                    translation.Status = ImportStatus.Failed;
                    translation.StatusMessage = "Import process failed";
                }
            }
            finally
            {
                IsImporting = false;
                StateHasChanged();
            }
        }

        private void ToggleSelectAll(bool isChecked)
        {
            if (ImportedTranslations != null)
            {
                foreach (var translation in ImportedTranslations)
                {
                    translation.ShouldImport = isChecked;
                }
            }
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
}
