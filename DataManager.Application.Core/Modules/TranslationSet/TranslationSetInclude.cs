namespace DataManager.Application.Core.Modules.TranslationSet;

public class TranslationSetInclude
{
    public Guid ParentTranslationSetId { get; set; }
    public TranslationSet ParentTranslationSet { get; set; } = null!;

    public Guid IncludedTranslationSetId { get; set; }
    public TranslationSet IncludedTranslationSet { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
}
