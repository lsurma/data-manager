namespace DataManager.Application.Contracts.Modules.TranslationsSet;

public record TranslationsSetDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// List of user/application identity IDs that have access to this TranslationsSet.
    /// If empty, the TranslationsSet has public access (no restrictions).
    /// </summary>
    public ICollection<string> AllowedIdentityIds { get; set; } = new List<string>();

    /// <summary>
    /// List of culture codes available for this TranslationsSet (e.g., "en-US", "pl-PL").
    /// If null or empty, all system cultures are available.
    /// </summary>
    public ICollection<string>? AvailableCultures { get; set; }

    public ICollection<Guid> IncludedTranslationsSetIds { get; set; } = new List<Guid>();

    public ICollection<TranslationsSetDto> IncludedTranslationsSets { get; set; } = new List<TranslationsSetDto>();

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}
