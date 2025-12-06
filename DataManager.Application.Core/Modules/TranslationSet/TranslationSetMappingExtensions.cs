using DataManager.Application.Contracts.Modules.TranslationSet;

namespace DataManager.Application.Core.Modules.TranslationSet;

public static class TranslationSetMappingExtensions
{
    public static TranslationSetDto ToDto(this TranslationSet translationSet)
    {
        return new TranslationSetDto
        {
            Id = translationSet.Id,
            Name = translationSet.Name,
            Description = translationSet.Description,
            Notes = translationSet.Notes,
            AllowedIdentityIds = translationSet.AllowedIdentityIds.ToList(),
            AvailableCultures = translationSet.AvailableCultures?.ToList(),
            IncludedTranslationSetIds = translationSet.Includes.Select(i => i.IncludedTranslationSetId).ToList(),
            CreatedAt = translationSet.CreatedAt,
            UpdatedAt = translationSet.UpdatedAt,
            CreatedBy = translationSet.CreatedBy
        };
    }

    public static List<TranslationSetDto> ToDto(this List<TranslationSet> translationSets)
    {
        var dtoMap = new Dictionary<Guid, TranslationSetDto>();

        // First pass: create all DTOs
        foreach (var translationSet in translationSets)
        {
            dtoMap[translationSet.Id] = translationSet.ToDto();
        }

        // Second pass: populate IncludedTranslationSets navigation property
        foreach (var translationSet in translationSets)
        {
            var dto = dtoMap[translationSet.Id];
            dto.IncludedTranslationSets = translationSet.Includes
                .Where(i => dtoMap.ContainsKey(i.IncludedTranslationSetId))
                .Select(i => dtoMap[i.IncludedTranslationSetId])
                .ToList();
        }

        return dtoMap.Values.ToList();
    }
}
