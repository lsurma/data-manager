using MediatR;

namespace DataManager.Application.Contracts.Modules.TranslationsSet;

public class UploadTranslationFileCommand : IRequest
{
    public Guid TranslationsSetId { get; set; }
    public string? FileName { get; set; }
    public Stream? Content { get; set; }
}