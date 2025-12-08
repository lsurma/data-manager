using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

public class ImportTranslationsCommandHandler : IRequestHandler<ImportTranslationsCommand, ImportTranslationsResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ImportTranslationsCommandHandler> _logger;
    private readonly TranslationsQueryService _queryService;
    private readonly DataManagerDbContext _context;

    public ImportTranslationsCommandHandler(
        IMediator mediator, 
        ILogger<ImportTranslationsCommandHandler> logger,
        TranslationsQueryService queryService,
        DataManagerDbContext context)
    {
        _mediator = mediator;
        _logger = logger;
        _queryService = queryService;
        _context = context;
    }

    public async Task<ImportTranslationsResult> Handle(ImportTranslationsCommand request, CancellationToken cancellationToken)
    {
        var result = new ImportTranslationsResult();

        // Prefetch all existing translations that might be updated during import
        // This loads them into the EF Core change tracker for efficient lookups
        await PrefetchExistingTranslationsAsync(request, cancellationToken);

        foreach (var translation in request.Translations)
        {
            try
            {
                // Use SaveSingleTranslationCommand for imports to preserve all detailed properties
                var saveCommand = new SaveSingleTranslationCommand
                {
                    ResourceName = translation.ResourceName,
                    TranslationName = translation.TranslationName,
                    CultureName = translation.CultureName ?? "en-US", // Default to en-US if not specified
                    Content = translation.Content,
                    DataSetId = request.DataSetId,
                    IsDraftVersion = false
                };

                await _mediator.Send(saveCommand, cancellationToken);
                result.ImportedCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add($"Failed to import translation '{translation.TranslationName}' ({translation.ResourceName}): {ex.Message}");
                _logger.LogError(ex, "Failed to import translation {TranslationName} ({ResourceName})",
                    translation.TranslationName, translation.ResourceName);
            }
        }

        return result;
    }

    /// <summary>
    /// Prefetches existing translations into the EF Core change tracker.
    /// This optimizes subsequent lookups in SaveSingleTranslationCommandHandler by avoiding database hits.
    /// Authorization is applied during prefetch, so no additional checks are needed.
    /// </summary>
    private async Task PrefetchExistingTranslationsAsync(
        ImportTranslationsCommand request, 
        CancellationToken cancellationToken)
    {
        // Build unique keys from import data to find existing translations
        var uniqueKeys = request.Translations
            .Select(t => new
            {
                ResourceName = t.ResourceName ?? string.Empty,
                TranslationName = t.TranslationName ?? string.Empty,
                CultureName = t.CultureName ?? "en-US"
            })
            .Distinct()
            .ToList();

        if (!uniqueKeys.Any())
        {
            return;
        }

        // Prepare authorized query with all necessary filters
        var query = await _queryService.PrepareQueryAsync(
            options: new QueryOptions<Translation, Guid>
            {
                AsNoTracking = false // We need tracking for local lookups
            },
            cancellationToken: cancellationToken);

        // Filter by DataSetId
        query = query.Where(t => t.DataSetId == request.DataSetId);

        // Filter to only current versions (matches logic in SaveSingleTranslationCommandHandler)
        query = query.Where(t => t.IsCurrentVersion);

        // Build a predicate to match any of the unique keys
        // Uses multiple Contains operations which translate to SQL IN clauses
        // This approach is efficient for typical import sizes (dozens to hundreds of translations)
        // and much better than N individual queries
        var resourceNames = uniqueKeys.Select(k => k.ResourceName).Distinct().ToList();
        var translationNames = uniqueKeys.Select(k => k.TranslationName).Distinct().ToList();
        var cultureNames = uniqueKeys.Select(k => k.CultureName).Distinct().ToList();

        query = query.Where(t =>
            resourceNames.Contains(t.ResourceName) &&
            translationNames.Contains(t.TranslationName) &&
            cultureNames.Contains(t.CultureName));

        // Execute query to load translations into change tracker
        // This is a single database hit instead of N hits during import
        var existingTranslations = await query.ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Prefetched {Count} existing translations for import optimization",
            existingTranslations.Count);
    }
}
