using MediatR;

namespace DataManager.Application.Contracts.Modules.TranslationsSet;

public class GetTranslationsSetByIdQuery : IRequest<TranslationsSetDto?>
{
    public Guid Id { get; set; }
}
