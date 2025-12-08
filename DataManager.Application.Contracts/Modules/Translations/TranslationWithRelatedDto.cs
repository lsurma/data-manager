namespace DataManager.Application.Contracts.Modules.Translations;

public record TranslationWithRelatedDto
{
    /// <summary>
    /// The main translation that was requested
    /// </summary>
    public TranslationDto MainTranslation { get; set; } = null!;

    /// <summary>
    /// All related translations with the same ResourceName and TranslationName across all cultures
    /// </summary>
    public List<TranslationDto> RelatedTranslations { get; set; } = new();
}
