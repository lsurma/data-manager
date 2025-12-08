using MediatR;

namespace DataManager.Application.Contracts.Modules.Translations;

public record GetTranslationWithRelatedQuery(Guid TranslationId) : IRequest<TranslationWithRelatedDto>;
