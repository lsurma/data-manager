using MediatR;

namespace DataManager.Application.Contracts.Modules.Translations;

public class GetTranslationByIdQuery : IRequest<TranslationDto?>
{
    public Guid Id { get; set; }
}
