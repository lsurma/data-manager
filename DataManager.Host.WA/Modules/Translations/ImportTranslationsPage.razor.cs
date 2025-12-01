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

        private async Task OnFileChanged(InputFileChangeEventArgs e)
        {
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
                            var dataTable = result.Tables[0];
                            _importedTranslations = dataTable.Rows.Cast<DataRow>().Select(row => new ImportedTranslationDto
                            {
                                InternalGroupName1 = row.Table.Columns.Contains("InternalGroupName1") ? row["InternalGroupName1"].ToString() : null,
                                InternalGroupName2 = row.Table.Columns.Contains("InternalGroupName2") ? row["InternalGroupName2"].ToString() : null,
                                ResourceName = row.Table.Columns.Contains("ResourceName") ? row["ResourceName"].ToString() : string.Empty,
                                TranslationName = row.Table.Columns.Contains("TranslationName") ? row["TranslationName"].ToString() : string.Empty,
                                CultureName = row.Table.Columns.Contains("CultureName") ? row["CultureName"].ToString() : null,
                                Content = row.Table.Columns.Contains("Content") ? row["Content"].ToString() : string.Empty,
                                OriginalRow = row
                            }).ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
