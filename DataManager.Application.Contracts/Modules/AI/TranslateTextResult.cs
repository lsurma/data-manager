namespace DataManager.Application.Contracts.Modules.AI;

/// <summary>
/// Result of a translation operation containing translations for all requested target cultures.
/// </summary>
public record TranslateTextResult
{
    /// <summary>
    /// Dictionary mapping culture codes to translated text.
    /// Key: Target culture code (e.g., "en-US", "pl-PL")
    /// Value: Translated text
    /// </summary>
    public required Dictionary<string, string> Translations { get; init; } = new();
}
