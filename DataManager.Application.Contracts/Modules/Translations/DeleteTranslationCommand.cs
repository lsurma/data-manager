using MediatR;

namespace DataManager.Application.Contracts.Modules.Translations;

public class DeleteTranslationCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
