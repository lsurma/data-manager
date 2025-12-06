using MediatR;

namespace DataManager.Application.Contracts.Modules.TranslationsSet;

/// <summary>
/// Query to get a translationsset hierarchy starting from a root translationsset.
/// Returns all included translationssets in breadth-first order.
/// Example: If "Final" includes "GlobalData", and "GlobalData" includes "A" and "B",
/// the result will be [Final, GlobalData, A, B].
/// </summary>
public record GetTranslationsSetHierarchyQuery : IRequest<TranslationsSetHierarchyDto>
{
    public required Guid RootTranslationsSetId { get; init; }
}
