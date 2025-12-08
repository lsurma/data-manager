using DataManager.Application.Core.Abstractions;
using DataManager.Application.Core.Modules.DataSets;

namespace DataManager.Application.Core.Modules.Translations;

public class Translation : AuditableEntityBase
{
    public string? InternalGroupName1 { get; set; }

    public string? InternalGroupName2 { get; set; }

    public required string ResourceName { get; set; }

    public required string TranslationName { get; set; }

    /// <summary>
    /// Lookup key built from ResourceName and TranslationName.
    /// Automatically populated before save.
    /// </summary>
    public string TranslationKey { get; set; } = string.Empty;

    public string? CultureName { get; set; }

    public required string Content { get; set; }

    /// <summary>
    /// Optional MJML template content before processing
    /// </summary>
    public string? ContentTemplate { get; set; }

    /// <summary>
    /// Timestamp of when the content (Content or ContentTemplate) was last updated.
    /// This is only updated when content changes, not when other properties change.
    /// </summary>
    public DateTimeOffset? ContentUpdatedAt { get; set; }

    public Guid? DataSetId { get; set; }

    public DataSet? DataSet { get; set; }

    /// <summary>
    /// Optional reference to the source Translation from which this translation was materialized.
    /// Used to determine if translation is "original" (null) or materialized from another dataset.
    /// Points directly to the source translation entity.
    /// </summary>
    public Guid? SourceTranslationId { get; set; }

    /// <summary>
    /// Navigation property to the source Translation
    /// </summary>
    public Translation? SourceTranslation { get; set; }

    /// <summary>
    /// Timestamp of last sync from the source Translation
    /// </summary>
    public DateTimeOffset? SourceTranslationLastSyncedAt { get; set; }

    /// <summary>
    /// Optional reference to a layout Translation (used for email templates)
    /// </summary>
    public Guid? LayoutId { get; set; }

    /// <summary>
    /// Navigation property to the layout Translation
    /// </summary>
    public Translation? Layout { get; set; }

    /// <summary>
    /// Optional reference to a source Translation (for linking similar/same translations)
    /// </summary>
    public Guid? SourceId { get; set; }

    /// <summary>
    /// Navigation property to the source Translation
    /// </summary>
    public Translation? Source { get; set; }

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
    /// Used to track version history
    /// </summary>
    public Guid? OriginalTranslationId { get; set; }

    /// <summary>
    /// Navigation property to the original Translation
    /// </summary>
    public Translation? OriginalTranslation { get; set; }
}
