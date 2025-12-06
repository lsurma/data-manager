using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Contracts.Modules.TranslationsSet;
using DataManager.Application.Core.Data;
using DataManager.Application.Core.Modules.TranslationsSet;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

/// <summary>
/// High-level handler for saving translations for multiple cultures.
/// Processes a dictionary of culture-to-content mappings and creates/updates translations accordingly.
/// </summary>
public class SaveTranslationCommandHandler : IRequestHandler<SaveTranslationCommand, Guid>
{
    private readonly DataManagerDbContext _context;
    private readonly ISender _mediator;

    public SaveTranslationCommandHandler(
        DataManagerDbContext context,
        ISender mediator
    )
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(SaveTranslationCommand request, CancellationToken cancellationToken)
    {
        // Determine ResourceName and TranslationName
        string resourceName;
        string translationName;
        Guid? translationsSetId = request.TranslationsSetId;

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            // Update existing - fetch ResourceName and TranslationName from existing translation
            var existingTranslation = await _context.Translations
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.Id.Value, cancellationToken);

            if (existingTranslation == null)
            {
                throw new KeyNotFoundException($"Translation with Id {request.Id} not found.");
            }

            resourceName = existingTranslation.ResourceName;
            translationName = existingTranslation.TranslationName;
            translationsSetId = existingTranslation.TranslationsSetId;
        }
        else
        {
            // Create new - use provided ResourceName and TranslationName
            if (string.IsNullOrWhiteSpace(request.ResourceName) || string.IsNullOrWhiteSpace(request.TranslationName))
            {
                throw new ArgumentException("ResourceName and TranslationName are required when creating new translations.");
            }

            resourceName = request.ResourceName;
            translationName = request.TranslationName;
        }

        if (translationsSetId == null)
        {
            throw new ArgumentException("TranslationsSetId must be provided either directly or via existing translation.");
        }

        var translationsSet = await _mediator.Send(
            new GetTranslationsSetByIdQuery() {
                Id = translationsSetId.Value
            },
            cancellationToken
        );
        
        if (translationsSet == null)
        {
            throw new ArgumentException($"TranslationsSet with Id {translationsSetId} not found.");
        }

        // Process each culture in the dictionary
        var processedTranslationIds = new List<Guid>();

        foreach (var (cultureName, content) in request.Translations)
        {
            // Find existing translation for this culture
            var existingTranslation = await _context.Translations
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                        t.ResourceName == resourceName &&
                        t.TranslationName == translationName &&
                        t.CultureName == cultureName &&
                        t.IsCurrentVersion,
                    cancellationToken);

            var command = new SaveSingleTranslationCommand {
                Id = existingTranslation?.Id,
                InternalGroupName1 = null,
                InternalGroupName2 = null,
                ResourceName = resourceName,
                TranslationName = translationName,
                CultureName = cultureName,
                Content = content,
                ContentTemplate = null,
                TranslationsSetId = request.TranslationsSetId,
                LayoutId = null,
                SourceId = null,
                IsDraftVersion = false
            };

            var translationId = await _mediator.Send(command, cancellationToken);
            processedTranslationIds.Add(translationId);
        }

        // Return the first processed translation ID
        return Guid.Empty;
    }
}