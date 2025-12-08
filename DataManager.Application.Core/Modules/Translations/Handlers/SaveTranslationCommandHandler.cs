using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Contracts.Modules.DataSets;
using DataManager.Application.Core.Common.BackgroundJobs;
using DataManager.Application.Core.Data;
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
    private readonly WebhookNotificationHelper _webhookHelper;

    public SaveTranslationCommandHandler(
        DataManagerDbContext context,
        ISender mediator,
        WebhookNotificationHelper webhookHelper
    )
    {
        _context = context;
        _mediator = mediator;
        _webhookHelper = webhookHelper;
    }

    public async Task<Guid> Handle(SaveTranslationCommand request, CancellationToken cancellationToken)
    {
        // Determine ResourceName and TranslationName
        string resourceName;
        string translationName;
        Guid? translationSetId = request.DataSetId;

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
            translationSetId = existingTranslation.DataSetId;
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

        if (translationSetId == null)
        {
            throw new ArgumentException("DataSetId must be provided either directly or via existing translation.");
        }

        var dataSet = await _mediator.Send(
            new GetDataSetByIdQuery() {
                Id = translationSetId.Value
            },
            cancellationToken
        );
        
        if (dataSet == null)
        {
            throw new ArgumentException($"DataSet with Id {translationSetId} not found.");
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
                        t.DataSetId == dataSet.Id &&
                        t.IsCurrentVersion,
                    cancellationToken);

            var command = new SaveSingleTranslationCommand {
                Id = existingTranslation?.Id,
                ResourceName = resourceName,
                TranslationName = translationName,
                CultureName = cultureName,
                Content = content,
                DataSetId = dataSet.Id,
                IsDraftVersion = false,
                
                InternalGroupName1 = default,
                InternalGroupName2 = default,
                ContentTemplate = default,
                LayoutId = default,
                SourceId = default,
            };

            var translationId = await _mediator.Send(command, cancellationToken);
            processedTranslationIds.Add(translationId);
        }

        // Notify webhooks that translations were updated
        // This is a fire-and-forget operation - we don't wait for webhook delivery
        _ = _webhookHelper.NotifyTranslationSetChangeAsync(
            translationSetId: translationSetId.Value,
            eventType: "translation.updated",
            additionalData: new Dictionary<string, object>
            {
                { "resourceName", resourceName },
                { "translationName", translationName },
                { "cultures", request.Translations.Keys.ToList() }
            },
            cancellationToken: cancellationToken
        );

        return request.Id ?? processedTranslationIds.FirstOrDefault();
    }
}