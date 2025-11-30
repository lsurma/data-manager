using MediatR;

namespace DataManager.Application.Contracts.Modules.Translations;

public class SaveTranslationCommand : IRequest<Guid>
{
    public Guid? Id { get; set; }

    public string? InternalGroupName1 { get; set; }

    public string? InternalGroupName2 { get; set; }

    public required string ResourceName { get; set; }

    public required string TranslationName { get; set; }

    public string? CultureName { get; set; }

    public required string Content { get; set; }

    /// <summary>
    /// Optional MJML template content before processing
    /// </summary>
    public string? ContentTemplate { get; set; }

    public Guid? DataSetId { get; set; }

    /// <summary>
    /// Optional reference to a layout Translation (used for email templates)
    /// </summary>
    public Guid? LayoutId { get; set; }

    /// <summary>
    /// Optional reference to a source Translation (for linking similar/same translations)
    /// </summary>
    public Guid? SourceId { get; set; }
}
