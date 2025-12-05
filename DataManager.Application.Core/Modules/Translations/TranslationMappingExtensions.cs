using DataManager.Application.Contracts.Modules.Translations;

namespace DataManager.Application.Core.Modules.Translations;

public static class TranslationMappingExtensions
{
    public static TranslationDto ToDto(this Translation translation)
    {
        return new TranslationDto
        {
            Id = translation.Id,
            InternalGroupName1 = translation.InternalGroupName1,
            InternalGroupName2 = translation.InternalGroupName2,
            ResourceName = translation.ResourceName,
            TranslationName = translation.TranslationName,
            CultureName = translation.CultureName,
            Content = translation.Content,
            ContentTemplate = translation.ContentTemplate,
            DataSetId = translation.DataSetId,
            SourceTranslationId = translation.SourceTranslationId,
            SourceTranslationLastSyncedAt = translation.SourceTranslationLastSyncedAt,
            LayoutId = translation.LayoutId,
            SourceId = translation.SourceId,
            IsCurrentVersion = translation.IsCurrentVersion,
            IsDraftVersion = translation.IsDraftVersion,
            IsOldVersion = translation.IsOldVersion,
            OriginalTranslationId = translation.OriginalTranslationId,
            CreatedAt = translation.CreatedAt,
            UpdatedAt = translation.UpdatedAt,
            CreatedBy = translation.CreatedBy
        };
    }

    public static List<TranslationDto> ToDto(this List<Translation> translations)
    {
        return translations.Select(t => t.ToDto()).ToList();
    }
}
