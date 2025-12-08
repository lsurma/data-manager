using DataManager.Application.Contracts.Common;
using MediatR;

namespace DataManager.Application.Contracts.Modules.Translations;

public class ExportTranslationsQuery : IRequest<Stream>
{
    public string? OrderBy { get; set; }
    public string? OrderDirection { get; set; }
    public string Format { get; set; } = "xlsx";
    
    public Guid DataSetId { get; set; }
    
    public string BaseCulture { get; set; } = "en-US";

    public string TargetCulture { get; set; } = "en-US";
}
