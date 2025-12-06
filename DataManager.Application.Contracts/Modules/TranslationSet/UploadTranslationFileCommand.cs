using MediatR;

namespace DataManager.Application.Contracts.Modules.TranslationSet;

public class UploadTranslationFileCommand : IRequest
{
    public Guid TranslationSetId { get; set; }
    public string? FileName { get; set; }
    public Stream? Content { get; set; }
}