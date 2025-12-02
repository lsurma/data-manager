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
using DataManager.Application.Contracts.Modules.DataSet;
using Microsoft.FluentUI.AspNetCore.Components;

namespace DataManager.Host.WA.Modules.Translations
{
    public partial class ImportTranslationsPage : ComponentBase
    {
        [Inject]
        private IRequestSender RequestSender { get; set; } = null!;

        [Inject]
        private IToastService ToastService { get; set; } = null!;

        private List<ImportedTranslationDto>? _importedTranslations;
        private bool _isLoading;
        private bool _isImporting;
        private string? _errorMessage;
        private DataTable? _excelDataTable;
        private readonly List<string> _targetColumns = new() { "InternalGroupName1", "InternalGroupName2", "ResourceName", "TranslationName", "CultureName", "Content" };
        private Dictionary<string, string> _columnMappings = new();
        private List<DataSetDto> _availableDataSets = new();
        private Guid? _selectedDataSetId;
        private DataSetDto? _selectedDataSet;
        private string? _selectedDataSetValue
        {
            get => _selectedDataSetId?.ToString();
            set
            {
                if (Guid.TryParse(value, out var id))
                {
                    _selectedDataSetId = id;
                }
            }
        }

        private IEnumerable<DataRow>? ExcelDataRows => _excelDataTable?.Rows.Cast<DataRow>();

        protected override async Task OnInitializedAsync()
        {
            await LoadDataSetsAsync();
        }

        private async Task LoadDataSetsAsync()
        {
            try
            {
                var result = await RequestSender.SendAsync(GetDataSetsQuery.AllItems());
                _availableDataSets = result.Items;
                if (_availableDataSets.Any())
                {
                    _selectedDataSetId = _availableDataSets.First().Id;
                }
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to load data sets: {ex.Message}";
            }
        }

        private async Task OnFileChanged(InputFileChangeEventArgs e)
        {
            _isLoading = true;
            _errorMessage = null;
            _importedTranslations = null;
            _excelDataTable = null;
            _columnMappings = new();

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
                            _excelDataTable = result.Tables[0];
                            foreach (DataColumn col in _excelDataTable.Columns)
                            {
                                _columnMappings.Add(col.ColumnName, _targetColumns.FirstOrDefault(t => t.Equals(col.ColumnName, StringComparison.OrdinalIgnoreCase)) ?? "");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ProcessMappedColumns()
        {
            if (_excelDataTable == null) return;

            _importedTranslations = _excelDataTable.Rows.Cast<DataRow>().Select(row =>
            {
                var dto = new ImportedTranslationDto { OriginalRow = row };
                foreach (var mapping in _columnMappings)
                {
                    if (!string.IsNullOrEmpty(mapping.Value) && _excelDataTable.Columns.Contains(mapping.Key))
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
        }

        private async Task StartImportAsync()
        {
            if (_importedTranslations == null || !_importedTranslations.Any() || !_selectedDataSetId.HasValue)
            {
                ToastService.ShowError("Please select a data set and map columns before importing.");
                return;
            }

            var translationsToImport = _importedTranslations.Where(t => t.ShouldImport).ToList();
            if (!translationsToImport.Any())
            {
                ToastService.ShowWarning("No translations selected for import.");
                return;
            }

            _isImporting = true;
            _errorMessage = null;

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
                    DataSetId = _selectedDataSetId.Value,
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
                foreach (var error in result.Errors)
                {
                    // Try to extract translation name and resource name from error message
                    var match = System.Text.RegularExpressions.Regex.Match(
                        error, 
                        @"Failed to import translation '([^']+)' \(([^)]+)\):");
                    
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
                        }
                    }
                }

                // Handle case where we have more failures than matched errors
                if (result.FailedCount > result.Errors.Count)
                {
                    // Some errors might not have been properly formatted
                    var unmatchedFailures = result.FailedCount - result.Errors.Count;
                    _errorMessage = $"Warning: {unmatchedFailures} translation(s) failed but could not be matched to specific items.";
                }

                ToastService.ShowSuccess($"Import completed: {result.ImportedCount} succeeded, {result.FailedCount} failed.");
            }
            catch (Exception ex)
            {
                _errorMessage = $"Import failed: {ex.Message}";
                ToastService.ShowError(_errorMessage);

                // Mark all in-progress as failed
                foreach (var translation in translationsToImport.Where(t => t.Status == ImportStatus.InProgress))
                {
                    translation.Status = ImportStatus.Failed;
                    translation.StatusMessage = "Import process failed";
                }
            }
            finally
            {
                _isImporting = false;
                StateHasChanged();
            }
        }

        private void ToggleSelectAll(bool isChecked)
        {
            if (_importedTranslations != null)
            {
                foreach (var translation in _importedTranslations)
                {
                    translation.ShouldImport = isChecked;
                }
            }
        }
    }
}
