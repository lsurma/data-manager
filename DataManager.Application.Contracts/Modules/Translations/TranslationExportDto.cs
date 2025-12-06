namespace DataManager.Application.Contracts.Modules.Translations;

/// <summary>
/// Lightweight DTO for translation export operations.
/// Contains only the essential fields needed for export, reducing memory footprint and export file size.
/// </summary>
public record TranslationExportDto
{
    /// <summary>
    /// Unique identifier of the translation
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Resource name (e.g., "MyApp.Resources")
    /// </summary>
    public string ResourceName { get; set; } = string.Empty;

    /// <summary>
    /// Translation key/name (e.g., "WelcomeMessage")
    /// </summary>
    public string TranslationName { get; set; } = string.Empty;

    /// <summary>
    /// Culture/language code (e.g., "en-US", "de-DE")
    /// </summary>
    public string? CultureName { get; set; }

    /// <summary>
    /// The actual translation content/text
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Optional internal grouping name (level 1)
    /// </summary>
    public string? InternalGroupName1 { get; set; }

    /// <summary>
    /// Optional internal grouping name (level 2)
    /// </summary>
    public string? InternalGroupName2 { get; set; }
}

/// <summary>
/// Key used for translation lookups by ResourceName and TranslationName
/// </summary>
public record TranslationKey(string ResourceName, string TranslationName);
