using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

/// <summary>
/// High-level handler for saving translations.
/// For new translations, creates entries for all cultures in the TranslationSet.
/// For existing translations, delegates to SaveSingleTranslationCommand.
/// </summary>
public class SaveTranslationCommandHandler : IRequestHandler<SaveTranslationCommand, Guid>
{
    private readonly DataManagerDbContext _context;
    private readonly ICultureService _cultureService;
    private readonly ISender _mediator;

    public SaveTranslationCommandHandler(
        DataManagerDbContext context,
        ICultureService cultureService,
        ISender mediator)
    {
        _context = context;
        _cultureService = cultureService;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(SaveTranslationCommand request, CancellationToken cancellationToken)
    {
        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            // Update existing - delegate to SaveSingleTranslationCommand
            // Note: CultureName from request is used (should match the existing translation's culture)
            var command = new SaveSingleTranslationCommand
            {
                Id = request.Id,
                InternalGroupName1 = request.InternalGroupName1,
                InternalGroupName2 = request.InternalGroupName2,
                ResourceName = request.ResourceName,
                TranslationName = request.TranslationName,
                CultureName = request.CultureName ?? throw new ArgumentException("CultureName is required when updating existing translation"),
                Content = request.Content,
                ContentTemplate = request.ContentTemplate,
                TranslationSetId = request.TranslationSetId,
                LayoutId = request.LayoutId,
                SourceId = request.SourceId,
                IsDraftVersion = request.IsDraftVersion
            };

            return await _mediator.Send(command, cancellationToken);
        }
        else
        {
            // Create new - automatically create for all cultures in the TranslationSet

            // Get available cultures based on TranslationSet configuration
            List<string> culturesToCreate;

            if (request.TranslationSetId.HasValue)
            {
                var translationSet = await _context.TranslationSets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ds => ds.Id == request.TranslationSetId.Value, cancellationToken);

                if (translationSet == null)
                {
                    throw new KeyNotFoundException($"TranslationSet with Id {request.TranslationSetId} not found.");
                }

                // If TranslationSet has specific cultures configured, use those; otherwise use all system cultures
                culturesToCreate = translationSet.AvailableCultures != null && translationSet.AvailableCultures.Any()
                    ? translationSet.AvailableCultures.ToList()
                    : _cultureService.GetAvailableCultures();
            }
            else
            {
                // No TranslationSet specified - use all system cultures
                culturesToCreate = _cultureService.GetAvailableCultures();
            }

            // Create translation for each culture using SaveSingleTranslationCommand
            var createdTranslationIds = new List<Guid>();

            foreach (var culture in culturesToCreate)
            {
                var command = new SaveSingleTranslationCommand
                {
                    Id = null, // New translation
                    InternalGroupName1 = request.InternalGroupName1,
                    InternalGroupName2 = request.InternalGroupName2,
                    ResourceName = request.ResourceName,
                    TranslationName = request.TranslationName,
                    CultureName = culture,
                    Content = request.Content,
                    ContentTemplate = request.ContentTemplate,
                    TranslationSetId = request.TranslationSetId,
                    LayoutId = request.LayoutId,
                    SourceId = request.SourceId,
                    IsDraftVersion = request.IsDraftVersion
                };

                var translationId = await _mediator.Send(command, cancellationToken);
                createdTranslationIds.Add(translationId);
            }

            // Return the first created translation ID
            return createdTranslationIds.First();
        }
    }
}
