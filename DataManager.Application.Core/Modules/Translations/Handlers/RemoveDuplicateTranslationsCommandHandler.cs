using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

/// <summary>
/// Handler for removing duplicate translations from a specific dataset that also exist in a base dataset.
/// Processes translations in batches of 250 for efficient memory usage.
/// </summary>
public class RemoveDuplicateTranslationsCommandHandler 
    : IRequestHandler<RemoveDuplicateTranslationsCommand, RemoveDuplicateTranslationsResult>
{
    private readonly DataManagerDbContext _context;
    private readonly TranslationsQueryService _queryService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<RemoveDuplicateTranslationsCommandHandler> _logger;
    private const int BatchSize = 250;

    public RemoveDuplicateTranslationsCommandHandler(
        DataManagerDbContext context,
        TranslationsQueryService queryService,
        IAuthorizationService authorizationService,
        ILogger<RemoveDuplicateTranslationsCommandHandler> logger)
    {
        _context = context;
        _queryService = queryService;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<RemoveDuplicateTranslationsResult> Handle(
        RemoveDuplicateTranslationsCommand request, 
        CancellationToken cancellationToken)
    {
        var removedCount = 0;
        var processedCount = 0;
        var errors = new List<string>();

        try
        {
            // Verify both datasets exist and are accessible
            var specificDataSetExists = await VerifyDataSetAccessAsync(request.SpecificDataSetId, cancellationToken);
            var baseDataSetExists = await VerifyDataSetAccessAsync(request.BaseDataSetId, cancellationToken);

            if (!specificDataSetExists)
            {
                errors.Add($"Specific DataSet with ID {request.SpecificDataSetId} not found or not accessible.");
                return new RemoveDuplicateTranslationsResult
                {
                    RemovedCount = 0,
                    ProcessedCount = 0,
                    Errors = errors
                };
            }

            if (!baseDataSetExists)
            {
                errors.Add($"Base DataSet with ID {request.BaseDataSetId} not found or not accessible.");
                return new RemoveDuplicateTranslationsResult
                {
                    RemovedCount = 0,
                    ProcessedCount = 0,
                    Errors = errors
                };
            }

            // Process translations in batches
            var hasMore = true;
            var skip = 0;

            while (hasMore)
            {
                // Fetch a batch of translations from the specific dataset
                var specificTranslationsBatch = await _queryService.PrepareQueryAsync(
                    query: _context.Translations.AsQueryable(),
                    cancellationToken: cancellationToken);

                specificTranslationsBatch = specificTranslationsBatch
                    .Where(t => t.DataSetId == request.SpecificDataSetId)
                    .OrderBy(t => t.Id)
                    .Skip(skip)
                    .Take(BatchSize);

                var batch = await specificTranslationsBatch
                    .Select(t => new
                    {
                        t.Id,
                        t.TranslationKey,
                        t.CultureName,
                        t.Content
                    })
                    .ToListAsync(cancellationToken);

                if (!batch.Any())
                {
                    hasMore = false;
                    break;
                }

                processedCount += batch.Count;

                // Find matching translations in the base dataset
                var translationKeys = batch.Select(t => t.TranslationKey).ToList();
                var cultureNames = batch.Select(t => t.CultureName).Distinct().ToList();
                var contents = batch.Select(t => t.Content).Distinct().ToList();

                var baseTranslations = await _queryService.PrepareQueryAsync(
                    query: _context.Translations.AsQueryable(),
                    cancellationToken: cancellationToken);

                baseTranslations = baseTranslations
                    .Where(t => t.DataSetId == request.BaseDataSetId)
                    .Where(t => translationKeys.Contains(t.TranslationKey))
                    .Where(t => cultureNames.Contains(t.CultureName))
                    .Where(t => contents.Contains(t.Content));

                // Use tuples for proper equality comparison
                var baseTranslationMatches = await baseTranslations
                    .Select(t => new ValueTuple<string, string?, string>(t.TranslationKey, t.CultureName, t.Content))
                    .ToListAsync(cancellationToken);

                var baseTranslationMatchesSet = baseTranslationMatches.ToHashSet();

                // Identify duplicates to delete
                var duplicateIds = batch
                    .Where(t => baseTranslationMatchesSet.Contains((t.TranslationKey, t.CultureName, t.Content)))
                    .Select(t => t.Id)
                    .ToList();

                if (duplicateIds.Any())
                {
                    // Delete duplicates
                    var translationsToDelete = await _context.Translations
                        .Where(t => duplicateIds.Contains(t.Id))
                        .ToListAsync(cancellationToken);

                    _context.Translations.RemoveRange(translationsToDelete);
                    await _context.SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);

                    removedCount += translationsToDelete.Count;

                    _logger.LogInformation(
                        "Removed {Count} duplicate translations from batch starting at {Skip}",
                        translationsToDelete.Count, skip);
                }
                else
                {
                    // No duplicates found in this batch, move to the next batch
                    skip += batch.Count;
                }

                // Check if we've processed all translations
                if (batch.Count < BatchSize)
                {
                    hasMore = false;
                }
            }

            _logger.LogInformation(
                "Completed removing duplicates. Processed: {ProcessedCount}, Removed: {RemovedCount}",
                processedCount, removedCount);

            return new RemoveDuplicateTranslationsResult
            {
                RemovedCount = removedCount,
                ProcessedCount = processedCount,
                Errors = errors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing duplicate translations");
            errors.Add($"Unexpected error: {ex.Message}");
            return new RemoveDuplicateTranslationsResult
            {
                RemovedCount = removedCount,
                ProcessedCount = processedCount,
                Errors = errors
            };
        }
    }

    /// <summary>
    /// Verifies that a dataset exists and is accessible by the current user
    /// </summary>
    private async Task<bool> VerifyDataSetAccessAsync(Guid dataSetId, CancellationToken cancellationToken)
    {
        // Check if dataset exists and user has access using the authorization service
        var canAccess = await _authorizationService.CanAccessDataSetAsync(dataSetId, cancellationToken);
        return canAccess;
    }
}
