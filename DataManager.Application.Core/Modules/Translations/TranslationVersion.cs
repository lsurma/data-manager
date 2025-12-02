using DataManager.Application.Core.Abstractions;

namespace DataManager.Application.Core.Modules.Translations;

public class TranslationVersion : AuditableEntityBase
{
    public Guid TranslationId { get; set; }
    public string? InternalGroupName1 { get; set; }
    public string? InternalGroupName2 { get; set; }
    public required string ResourceName { get; set; }
    public required string TranslationName { get; set; }
    public string? CultureName { get; set; }
    public required string Content { get; set; }
    public string? ContentTemplate { get; set; }
    public Guid? DataSetId { get; set; }
    public Guid? LayoutId { get; set; }
    public Guid? SourceId { get; set; }
}
