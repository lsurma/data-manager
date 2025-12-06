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

        var baseCulture = parameters.ContainsKey("BaseCulture") ? parameters["BaseCulture"].ToString() : "-";
        var targetCulture = parameters.ContainsKey("TargetCulture") ? parameters["TargetCulture"].ToString() : "-";
        var baseTranslations = parameters.ContainsKey("BaseTranslations") 
            ? parameters["BaseTranslations"] as IEnumerable<TranslationExportDto> 
            : null;

        // Create a lookup for base language translations by key
        // Base translations are already filtered by culture in the handler, so no need to filter again
        var baseTranslationsLookup = baseTranslations?
            .GroupBy(t => new TranslationKey(t.ResourceName, t.TranslationName))
            .ToDictionary(
                g => g.Key,
                g => g.First().Content
            );

        // Set up headers
        worksheet.Cell(1, 1).Value = "ResourceName";
        worksheet.Cell(1, 2).Value = "TranslationName";
        worksheet.Cell(1, 3).Value = $"Content ({baseCulture})";
        worksheet.Cell(1, 4).Value = $"Content ({targetCulture})";

        // Style headers
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        foreach (var translation in translations)
        {
            worksheet.Cell(row, 1).Value = translation.ResourceName;
            worksheet.Cell(row, 2).Value = translation.TranslationName;
            
            // Add base language translation if available
            if (baseTranslationsLookup != null)
            {
                var key = new TranslationKey(translation.ResourceName, translation.TranslationName);
                if (baseTranslationsLookup.TryGetValue(key, out var baseContent))
                {
                    worksheet.Cell(row, 3).Value = baseContent;
                }
            }
            
            worksheet.Cell(row, 4).Value = translation.Content;

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