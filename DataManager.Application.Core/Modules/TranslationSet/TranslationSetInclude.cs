namespace DataManager.Application.Core.Modules.TranslationSet;

public class TranslationSetInclude
{
    public Guid ParentTranslationSetId { get; set; }
    public TranslationSet ParentDataSet { get; set; } = null!;

    public Guid IncludedTranslationSetId { get; set; }
    public TranslationSet IncludedDataSet { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
}
