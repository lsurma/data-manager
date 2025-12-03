using ClosedXML.Excel;
using DataManager.Application.Contracts.Modules.Translations;

namespace DataManager.Application.Core.Common;

public class ExcelExporterService : ITranslationExporter
{
    public string Format => "xlsx";

    public Task<Stream> ExportAsync(IEnumerable<TranslationDto> translations, IDictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Translations");

        var baseLanguage = parameters.ContainsKey("BaseLanguage") ? parameters["BaseLanguage"].ToString() : "en-US";

        worksheet.Cell(1, 1).Value = "Key";
        worksheet.Cell(1, 2).Value = baseLanguage;

        var cultures = translations.SelectMany(t => t.Values.Keys).Distinct().Where(c => c != baseLanguage).OrderBy(c => c).ToList();
        for (var i = 0; i < cultures.Count; i++)
        {
            worksheet.Cell(1, i + 3).Value = cultures[i];
        }

        var row = 2;
        foreach (var translation in translations)
        {
            worksheet.Cell(row, 1).Value = translation.Key;
            worksheet.Cell(row, 2).Value = translation.Values.TryGetValue(baseLanguage, out var baseValue) ? baseValue : string.Empty;

            for (var i = 0; i < cultures.Count; i++)
            {
                worksheet.Cell(row, i + 3).Value = translation.Values.TryGetValue(cultures[i], out var value) ? value : string.Empty;
            }

            row++;
        }

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return Task.FromResult<Stream>(stream);
    }
}