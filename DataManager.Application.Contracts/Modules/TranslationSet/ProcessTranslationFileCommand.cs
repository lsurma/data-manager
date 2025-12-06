using MediatR;

namespace DataManager.Application.Contracts.Modules.TranslationSet;

public class ProcessTranslationFileCommand : IRequest
{
    public Guid TranslationSetId { get; set; }
    public string? FileName { get; set; }
}