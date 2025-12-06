using MediatR;

namespace DataManager.Application.Contracts.Modules.TranslationsSet;

public class ProcessTranslationFileCommand : IRequest
{
    public Guid TranslationsSetId { get; set; }
    public string? FileName { get; set; }
}