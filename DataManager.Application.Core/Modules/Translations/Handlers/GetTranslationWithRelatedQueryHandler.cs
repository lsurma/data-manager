using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

public class GetTranslationWithRelatedQueryHandler : IRequestHandler<GetTranslationWithRelatedQuery, TranslationWithRelatedDto>
{
    private readonly DataManagerDbContext _context;
    private readonly TranslationsQueryService _translationsQueryService;

    public GetTranslationWithRelatedQueryHandler(DataManagerDbContext context, TranslationsQueryService translationsQueryService)
    {
        _context = context;
        _translationsQueryService = translationsQueryService;
    }

    public async Task<TranslationWithRelatedDto> Handle(GetTranslationWithRelatedQuery request, CancellationToken cancellationToken)
    {
        // First, get the main translation
        var mainTranslation = await _translationsQueryService.GetByIdAsync(
            request.TranslationId,
            options: new TranslationsQueryService.Options(),
            cancellationToken: cancellationToken
        );
        
        if (mainTranslation == null)
        {
            throw new KeyNotFoundException($"Translation with ID {request.TranslationId} not found.");
        }

        // Get all related translations with the same ResourceName and TranslationName
        // Filter to only current versions (not old/archived versions)
        var relatedTranslations = await _context.Translations
            .AsNoTracking()
            .Where(t => t.TranslationKey == mainTranslation.TranslationKey && t.DataSetId == mainTranslation.DataSetId && t.IsCurrentVersion)
            .OrderBy(t => t.CultureName)
            .ToListAsync(cancellationToken);

        return new TranslationWithRelatedDto
        {
            MainTranslation = mainTranslation.ToDto(),
            RelatedTranslations = relatedTranslations.ToDto()
        };
    }
}
