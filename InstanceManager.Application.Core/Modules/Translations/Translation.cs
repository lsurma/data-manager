using InstanceManager.Application.Core.Abstractions;

namespace InstanceManager.Application.Core.Modules.Translations;

public class Translation : AuditableEntityBase
{
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

    public DataSet.DataSet? DataSet { get; set; }
}
