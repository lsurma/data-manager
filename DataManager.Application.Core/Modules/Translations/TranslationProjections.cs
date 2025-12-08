using System.Linq.Expressions;
using DataManager.Application.Contracts.Modules.Translations;

namespace DataManager.Application.Core.Modules.Translations;

/// <summary>
/// Static projection selectors for Translation entities
/// Add a new static method here for each projection type you want to support
/// </summary>
public static class TranslationProjections
{
    /// <summary>
    /// Full TranslationDto projection with all fields
    /// </summary>
    public static Expression<Func<Translation, TranslationDto>> ToTranslationDto()
    {
        return t => new TranslationDto
        {
            Id = t.Id,
            InternalGroupName1 = t.InternalGroupName1,
            InternalGroupName2 = t.InternalGroupName2,
            ResourceName = t.ResourceName,
            TranslationName = t.TranslationName,
            CultureName = t.CultureName,
            Content = t.Content,
            ContentTemplate = t.ContentTemplate,
            DataSetId = t.DataSetId,
            SourceTranslationId = t.SourceTranslationId,
            SourceTranslationLastSyncedAt = t.SourceTranslationLastSyncedAt,
            LayoutId = t.LayoutId,
            SourceId = t.SourceId,
            IsCurrentVersion = t.IsCurrentVersion,
            IsDraftVersion = t.IsDraftVersion,
            IsOldVersion = t.IsOldVersion,
            OriginalTranslationId = t.OriginalTranslationId,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            CreatedBy = t.CreatedBy
        };
    }

    /// <summary>
    /// Simple TranslationDto projection with only essential fields
    /// </summary>
    public static Expression<Func<Translation, SimpleTranslationDto>> ToSimpleTranslationDto()
    {
        return t => new SimpleTranslationDto
        {
            Id = t.Id,
            ResourceName = t.ResourceName,
            TranslationName = t.TranslationName,
            CultureName = t.CultureName,
            Content = t.Content,
            ContentUpdatedAt = t.ContentUpdatedAt
        };
    }

    /// <summary>
    /// Gets the appropriate selector for the given projection type
    /// Returns the expression as object - you'll need to cast it to the correct type in the handler
    /// </summary>
    public static object? GetSelectorFor(Type projectionType)
    {
        if (projectionType == typeof(TranslationDto))
        {
            return ToTranslationDto();
        }

        if (projectionType == typeof(SimpleTranslationDto))
        {
            return ToSimpleTranslationDto();
        }

        throw new NotSupportedException($"No projection selector defined for type {projectionType.Name}. " +
                                       $"Add a static method to TranslationProjections class.");
    }
}
