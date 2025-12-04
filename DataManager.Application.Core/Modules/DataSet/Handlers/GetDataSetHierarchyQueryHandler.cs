using DataManager.Application.Contracts.Modules.DataSet;
using MediatR;

namespace DataManager.Application.Core.Modules.DataSet.Handlers;

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
        // Get all datasets in the hierarchy using the new helper method
        var datasets = await _queryService.GetDataSetHierarchyAsync(
            request.RootDataSetId,
            cancellationToken);

        // Map to DTOs
        var datasetDtos = datasets.Select(ds => ds.ToDto()).ToList();

        return new DataSetHierarchyDto
        {
            RootDataSetId = request.RootDataSetId,
            DataSets = datasetDtos
        };
    }
}
