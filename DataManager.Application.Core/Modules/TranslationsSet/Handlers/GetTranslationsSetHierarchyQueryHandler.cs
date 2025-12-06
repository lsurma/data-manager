using DataManager.Application.Contracts.Modules.TranslationsSet;
using MediatR;

namespace DataManager.Application.Core.Modules.TranslationsSet.Handlers;

/// <summary>
/// Handler for GetTranslationsSetHierarchyQuery.
/// Retrieves a dataset hierarchy in breadth-first order using TranslationsSetsQueryService.
/// </summary>
public class GetTranslationsSetHierarchyQueryHandler : IRequestHandler<GetTranslationsSetHierarchyQuery, TranslationsSetHierarchyDto>
{
    private readonly TranslationsSetsQueryService _queryService;

    public GetTranslationsSetHierarchyQueryHandler(TranslationsSetsQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<TranslationsSetHierarchyDto> Handle(
        GetTranslationsSetHierarchyQuery request,
        CancellationToken cancellationToken)
    {
        // Get all translationssets in the hierarchy using the new helper method
        var translationsSets = await _queryService.GetTranslationsSetHierarchyAsync(
            request.RootTranslationsSetId,
            cancellationToken);

        // Map to DTOs using the optimized List extension method
        // This properly populates IncludedTranslationsSets navigation properties
        var translationsSetDtos = translationsSets.ToDto();

        return new TranslationsSetHierarchyDto
        {
            RootTranslationsSetId = request.RootTranslationsSetId,
            TranslationsSets = translationsSetDtos
        };
    }
}
