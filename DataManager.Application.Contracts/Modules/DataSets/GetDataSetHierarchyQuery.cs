using MediatR;

namespace DataManager.Application.Contracts.Modules.DataSets;

/// <summary>
/// Query to get a translationsset hierarchy starting from a root translationsset.
/// Returns all included translationssets in breadth-first order.
/// Example: If "Final" includes "GlobalData", and "GlobalData" includes "A" and "B",
/// the result will be [Final, GlobalData, A, B].
/// </summary>
public record GetDataSetHierarchyQuery : IRequest<DataSetHierarchyDto>
{
    public required Guid RootDataSetId { get; init; }
}
