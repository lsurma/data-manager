using DataManager.Application.Contracts.Modules.TranslationsSet;

namespace DataManager.Application.Core.Modules.TranslationsSet;

public static class TranslationsSetMappingExtensions
{
    public static TranslationsSetDto ToDto(this TranslationsSet translationsSet)
    {
        return new TranslationsSetDto
        {
            Id = translationsSet.Id,
            Name = translationsSet.Name,
            Description = translationsSet.Description,
            Notes = translationsSet.Notes,
            AllowedIdentityIds = translationsSet.AllowedIdentityIds.ToList(),
            AvailableCultures = translationsSet.AvailableCultures.ToList(),
            SecretKey = translationsSet.SecretKey,
            WebhookUrls = translationsSet.WebhookUrls.Select(uri => uri.ToString()).ToList(),
            IncludedTranslationsSetIds = translationsSet.Includes.Select(i => i.IncludedTranslationsSetId).ToList(),
            CreatedAt = translationsSet.CreatedAt,
            UpdatedAt = translationsSet.UpdatedAt,
            CreatedBy = translationsSet.CreatedBy
        };
    }

    public static List<TranslationsSetDto> ToDto(this List<TranslationsSet> translationsSets)
    {
        var dtoMap = new Dictionary<Guid, TranslationsSetDto>();

        // First pass: create all DTOs
        foreach (var translationsSet in translationsSets)
        {
            dtoMap[translationsSet.Id] = translationsSet.ToDto();
        }

        // Second pass: populate IncludedTranslationsSets navigation property
        foreach (var translationsSet in translationsSets)
        {
            var dto = dtoMap[translationsSet.Id];
            dto.IncludedTranslationsSets = translationsSet.Includes
                .Where(i => dtoMap.ContainsKey(i.IncludedTranslationsSetId))
                .Select(i => dtoMap[i.IncludedTranslationsSetId])
                .ToList();
        }

        return dtoMap.Values.ToList();
    }
}
