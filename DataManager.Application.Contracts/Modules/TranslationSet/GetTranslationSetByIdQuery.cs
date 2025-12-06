using MediatR;

namespace DataManager.Application.Contracts.Modules.TranslationSet;

public class GetTranslationSetByIdQuery : IRequest<TranslationSetDto?>
{
    public Guid Id { get; set; }
}
