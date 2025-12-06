using ClosedXML.Excel;
using DataManager.Application.Contracts.Modules.Translations;

namespace DataManager.Application.Core.Common;

public class ExcelExporterService : ITranslationExporter
{
    public string Format => "xlsx";

    public Task<Stream> ExportAsync(IEnumerable<TranslationExportDto> translations, IDictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Translations");

        var baseLanguage = parameters.ContainsKey("BaseLanguage") ? parameters["BaseLanguage"].ToString() : "en-US";
        var baseTranslations = parameters.ContainsKey("BaseTranslations") 
            ? parameters["BaseTranslations"] as IEnumerable<TranslationExportDto> 
            : null;

        // Create a lookup for base language translations by key
        var baseTranslationsLookup = baseTranslations?
            .Where(t => t.CultureName == baseLanguage)
            .GroupBy(t => new { t.ResourceName, t.TranslationName })
            .ToDictionary(
                g => g.Key,
                g => g.First().Content
            );

        // Set up headers
        worksheet.Cell(1, 1).Value = "ResourceName";
        worksheet.Cell(1, 2).Value = "TranslationName";
        worksheet.Cell(1, 3).Value = "CultureName";
        worksheet.Cell(1, 4).Value = baseLanguage;
        worksheet.Cell(1, 5).Value = "Content";
        worksheet.Cell(1, 6).Value = "InternalGroupName1";
        worksheet.Cell(1, 7).Value = "InternalGroupName2";

        // Style headers
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        foreach (var translation in translations)
        {
            worksheet.Cell(row, 1).Value = translation.ResourceName;
            worksheet.Cell(row, 2).Value = translation.TranslationName;
            worksheet.Cell(row, 3).Value = translation.CultureName ?? string.Empty;
            
            // Add base language translation if available
            if (baseTranslationsLookup != null)
            {
                var key = new { translation.ResourceName, translation.TranslationName };
                if (baseTranslationsLookup.TryGetValue(key, out var baseContent))
                {
                    worksheet.Cell(row, 4).Value = baseContent;
                }
            }
            
            worksheet.Cell(row, 5).Value = translation.Content;
            worksheet.Cell(row, 6).Value = translation.InternalGroupName1 ?? string.Empty;
            worksheet.Cell(row, 7).Value = translation.InternalGroupName2 ?? string.Empty;

            row++;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return Task.FromResult<Stream>(stream);
    }
}