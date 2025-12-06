using MediatR;

namespace DataManager.Application.Contracts.Modules.TranslationsSet;

public class DeleteTranslationsSetCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
