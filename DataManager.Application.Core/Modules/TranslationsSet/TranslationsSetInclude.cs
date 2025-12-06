namespace DataManager.Application.Core.Modules.TranslationsSet;

public class TranslationsSetInclude
{
    public Guid ParentTranslationsSetId { get; set; }
    public TranslationsSet ParentTranslationsSet { get; set; } = null!;

    public Guid IncludedTranslationsSetId { get; set; }
    public TranslationsSet IncludedTranslationsSet { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
}
