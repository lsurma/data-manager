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

namespace DataManager.Host.WA.Modules.Translations
{
    public partial class ImportTranslationsPage : ComponentBase
    {
        private List<ImportedTranslationDto>? _importedTranslations;
        private bool _isLoading;
        private string? _errorMessage;
        private DataTable? _excelDataTable;
        private readonly List<string> _targetColumns = new() { "InternalGroupName1", "InternalGroupName2", "ResourceName", "TranslationName", "CultureName", "Content" };
        private Dictionary<string, string> _columnMappings = new();

        private IEnumerable<DataRow>? ExcelDataRows => _excelDataTable?.Rows.Cast<DataRow>();

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
    }
}
