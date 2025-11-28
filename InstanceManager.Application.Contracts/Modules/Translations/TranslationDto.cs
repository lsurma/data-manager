namespace InstanceManager.Application.Contracts.Modules.Translations;

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
    /// Optional reference to a layout Translation (used for email templates)
    /// </summary>
    public Guid? LayoutId { get; set; }

    /// <summary>
    /// Optional reference to a source Translation (for linking similar/same translations)
    /// </summary>
    public Guid? SourceId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}
