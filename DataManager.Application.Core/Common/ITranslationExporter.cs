using DataManager.Application.Contracts.Modules.Translations;

namespace DataManager.Application.Core.Common;

public interface ITranslationExporter
{
    string Format { get; }
    Task<Stream> ExportAsync(IEnumerable<TranslationExportDto> translations, IDictionary<string, object> parameters, CancellationToken cancellationToken);
}