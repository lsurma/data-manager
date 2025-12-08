using MediatR;

namespace DataManager.Application.Contracts.Modules.Translations;

/// <summary>
/// High-level command to save translations for multiple cultures.
/// Simplifies the API by accepting a dictionary of culture-to-content mappings.
/// </summary>
public class SaveTranslationCommand : IRequest<Guid>
{
    /// <summary>
    /// Optional ID for updating existing translation group.
    /// If provided, ResourceName and TranslationName will be ignored.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Resource name for identifying the translation.
    /// Required when creating new translations (when Id is null).
    /// </summary>
    public string? ResourceName { get; set; }

    /// <summary>
    /// Translation key/name for identifying the translation.
    /// Required when creating new translations (when Id is null).
    /// </summary>
    public string? TranslationName { get; set; }

    /// <summary>
    /// Dictionary mapping culture codes to translation content.
    /// Key: Culture code (e.g., "en-US", "pl-PL")
    /// Value: Translation content
    /// </summary>
    public required Dictionary<string, string> Translations { get; set; }

    /// <summary>
    /// Optional DataSet ID to associate translations with a specific data set.
    /// </summary>
    public Guid? DataSetId { get; set; }
}
