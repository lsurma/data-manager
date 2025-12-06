namespace DataManager.Application.Contracts.Modules.TranslationsSet;

/// <summary>
/// DTO containing a translationsset and its full hierarchy of included translationssets.
/// TranslationsSets are ordered in breadth-first traversal order.
/// </summary>
public record TranslationsSetHierarchyDto
{
    public Guid RootTranslationsSetId { get; init; }
    
    /// <summary>
    /// List of translationssets in hierarchical order, starting with the root translationsset.
    /// Example: [Final, GlobalData, A, B] where Final includes GlobalData,
    /// and GlobalData includes A and B.
    /// </summary>
    public List<TranslationsSetDto> TranslationsSets { get; init; } = new();
}
