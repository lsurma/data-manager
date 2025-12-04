using MediatR;

namespace DataManager.Application.Contracts.Modules.DataSet;

/// <summary>
/// Query to get a dataset hierarchy starting from a root dataset.
/// Returns all included datasets in breadth-first order.
/// Example: If "Final" includes "GlobalData", and "GlobalData" includes "A" and "B",
/// the result will be [Final, GlobalData, A, B].
/// </summary>
public record GetDataSetHierarchyQuery : IRequest<DataSetHierarchyDto>
{
    public required Guid RootDataSetId { get; init; }
}
