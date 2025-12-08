using DataManager.Application.Contracts.Modules.DataSets;
using MediatR;

namespace DataManager.Application.Core.Modules.DataSets.Handlers;

/// <summary>
/// Handler for GetDataSetHierarchyQuery.
/// Retrieves a dataset hierarchy in breadth-first order using DataSetsQueryService.
/// </summary>
public class GetDataSetHierarchyQueryHandler : IRequestHandler<GetDataSetHierarchyQuery, DataSetHierarchyDto>
{
    private readonly DataSetsQueryService _queryService;

    public GetDataSetHierarchyQueryHandler(DataSetsQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<DataSetHierarchyDto> Handle(
        GetDataSetHierarchyQuery request,
        CancellationToken cancellationToken)
    {
        // Get all translationssets in the hierarchy using the new helper method
        var dataSets = await _queryService.GetDataSetHierarchyAsync(
            request.RootDataSetId,
            cancellationToken);

        // Map to DTOs using the optimized List extension method
        // This properly populates IncludedDataSets navigation properties
        var translationSetDtos = dataSets.ToDto();

        return new DataSetHierarchyDto
        {
            RootDataSetId = request.RootDataSetId,
            DataSets = translationSetDtos
        };
    }
}
