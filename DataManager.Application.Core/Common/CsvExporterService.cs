using System.Globalization;
using CsvHelper;
using DataManager.Application.Contracts.Modules.Translations;

namespace DataManager.Application.Core.Common;

public class CsvExporterService : ITranslationExporter
{
    public string Format => "csv";

    public async Task<Stream> ExportAsync(IEnumerable<TranslationExportDto> translations, IDictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            await csv.WriteRecordsAsync(translations, cancellationToken);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }
}