namespace DataManager.Application.Contracts.Modules.Translations;

public record TranslationDto : ITranslationDto
{
    public Guid Id { get; set; }

    public string? InternalGroupName1 { get; set; }

    public string? InternalGroupName2 { get; set; }

    public string ResourceName { get; set; } = string.Empty;

    public string TranslationName { get; set; } = string.Empty;

    public string? CultureName { get; set; }

    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Optional MJML template content before processing
    /// </summary>
    public string? ContentTemplate { get; set; }

    public Guid? DataSetId { get; set; }

    /// <summary>
    /// Optional reference to the source DataSet where this translation was fetched from.
    /// Used to determine if translation is "original" (null) or fetched from another dataset.
    /// </summary>
    public Guid? SourceDataSetId { get; set; }

    /// <summary>
    /// Timestamp of last sync from the source DataSet
    /// </summary>
    public DateTimeOffset? SourceDataSetLastSyncedAt { get; set; }

    /// <summary>
    /// Optional reference to a layout Translation (used for email templates)
    /// </summary>
    public Guid? LayoutId { get; set; }

    /// <summary>
    /// Optional reference to a source Translation (for linking similar/same translations)
    /// </summary>
    public Guid? SourceId { get; set; }

    /// <summary>
    /// Indicates if this is the current/active version of the translation
    /// </summary>
    public bool IsCurrentVersion { get; set; }

    /// <summary>
    /// Indicates if this is a draft version (not yet published)
    /// </summary>
    public bool IsDraftVersion { get; set; }

    /// <summary>
    /// Indicates if this is an old/archived version
    /// </summary>
    public bool IsOldVersion { get; set; }

    /// <summary>
    /// Reference to the original Translation when this is an old version
    /// </summary>
    public Guid? OriginalTranslationId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}
