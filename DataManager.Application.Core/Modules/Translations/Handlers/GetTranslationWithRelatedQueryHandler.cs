using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

public class GetTranslationWithRelatedQueryHandler : IRequestHandler<GetTranslationWithRelatedQuery, TranslationWithRelatedDto>
{
    private readonly DataManagerDbContext _context;

    public GetTranslationWithRelatedQueryHandler(DataManagerDbContext context)
    {
        _context = context;
    }

    public async Task<TranslationWithRelatedDto> Handle(GetTranslationWithRelatedQuery request, CancellationToken cancellationToken)
    {
        // First, get the main translation
        var mainTranslation = await _context.Translations
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TranslationId, cancellationToken);

        if (mainTranslation == null)
        {
            throw new KeyNotFoundException($"Translation with ID {request.TranslationId} not found.");
        }

        // Get all related translations with the same ResourceName and TranslationName
        // Filter to only current versions (not old/archived versions)
        var relatedTranslations = await _context.Translations
            .AsNoTracking()
            .Where(t => t.ResourceName == mainTranslation.ResourceName
                        && t.TranslationName == mainTranslation.TranslationName
                        && t.IsCurrentVersion)
            .OrderBy(t => t.CultureName)
            .ToListAsync(cancellationToken);

        return new TranslationWithRelatedDto
        {
            MainTranslation = mainTranslation.ToDto(),
            RelatedTranslations = relatedTranslations.ToDto()
        };
    }
}
