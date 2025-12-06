using DataManager.Application.Contracts.Modules.TranslationSet;
using MediatR;

namespace DataManager.Application.Core.Modules.TranslationSet.Handlers;

/// <summary>
/// Handler for GetTranslationSetHierarchyQuery.
/// Retrieves a dataset hierarchy in breadth-first order using TranslationSetsQueryService.
/// </summary>
public class GetTranslationSetHierarchyQueryHandler : IRequestHandler<GetTranslationSetHierarchyQuery, TranslationSetHierarchyDto>
{
    private readonly TranslationSetsQueryService _queryService;

    public GetTranslationSetHierarchyQueryHandler(TranslationSetsQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<TranslationSetHierarchyDto> Handle(
        GetTranslationSetHierarchyQuery request,
        CancellationToken cancellationToken)
    {
        // Get all datasets in the hierarchy using the new helper method
        var datasets = await _queryService.GetDataSetHierarchyAsync(
            request.RootTranslationSetId,
            cancellationToken);

        // Map to DTOs using the optimized List extension method
        // This properly populates IncludedTranslationSets navigation properties
        var datasetDtos = datasets.ToDto();

        return new TranslationSetHierarchyDto
        {
            RootTranslationSetId = request.RootTranslationSetId,
            TranslationSets = datasetDtos
        };
    }
}
