using DataManager.Application.Contracts.Common;
using MediatR;

namespace DataManager.Application.Contracts.Modules.Translations;

/// <summary>
/// Command to save a single translation for a specific culture.
/// This is a low-level command used internally to handle versioning and single translation persistence.
/// Uses Optional&lt;T&gt; properties to distinguish between unspecified and explicitly set values.
/// </summary>
public class SaveSingleTranslationCommand : IRequest<Guid>
{
    public Guid? Id { get; set; }

    public Optional<string?> InternalGroupName1 { get; set; }

    public Optional<string?> InternalGroupName2 { get; set; }

    public Optional<string> ResourceName { get; set; }

    public Optional<string> TranslationName { get; set; }

    public Optional<string?> CultureName { get; set; }

    public Optional<string> Content { get; set; }

    /// <summary>
    /// Optional MJML template content before processing
    /// </summary>
    public Optional<string?> ContentTemplate { get; set; }

    public Optional<Guid?> DataSetId { get; set; }

    /// <summary>
    /// Optional reference to a layout Translation (used for email templates)
    /// </summary>
    public Optional<Guid?> LayoutId { get; set; }

    /// <summary>
    /// Optional reference to a source Translation (for linking similar/same translations)
    /// </summary>
    public Optional<Guid?> SourceId { get; set; }

    /// <summary>
    /// Indicates if this is a draft version (not yet published)
    /// </summary>
    public Optional<bool> IsDraftVersion { get; set; }

    /// <summary>
    /// Optional timestamp of when the content was last updated.
    /// If specified and older than the existing entity's ContentUpdatedAt, the update will be rejected.
    /// </summary>
    public Optional<DateTimeOffset?> ContentUpdatedAt { get; set; }
}
