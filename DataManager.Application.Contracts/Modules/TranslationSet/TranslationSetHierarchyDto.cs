namespace DataManager.Application.Contracts.Modules.TranslationSet;

/// <summary>
/// DTO containing a dataset and its full hierarchy of included datasets.
/// Datasets are ordered in breadth-first traversal order.
/// </summary>
public record TranslationSetHierarchyDto
{
    public Guid RootTranslationSetId { get; init; }
    
    /// <summary>
    /// List of datasets in hierarchical order, starting with the root dataset.
    /// Example: [Final, GlobalData, A, B] where Final includes GlobalData,
    /// and GlobalData includes A and B.
    /// </summary>
    public List<TranslationSetDto> TranslationSets { get; init; } = new();
}
