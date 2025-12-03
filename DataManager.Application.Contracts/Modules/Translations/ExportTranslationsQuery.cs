using DataManager.Application.Contracts.Common;
using MediatR;

namespace DataManager.Application.Contracts.Modules.Translations;

public class ExportTranslationsQuery : IRequest<Stream>
{
    public string? OrderBy { get; set; }
    public string? OrderDirection { get; set; }
    public FilteringParameters? Filtering { get; set; }
    public string Format { get; set; } = "csv";
    public string ExportType { get; set; } = "All";
    public bool UseCurrentFilters { get; set; }
    public string BaseLanguage { get; set; } = "en-US";
}
