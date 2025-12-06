using MediatR;

namespace DataManager.Application.Contracts.Modules.TranslationSet;

public class DeleteTranslationSetCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
