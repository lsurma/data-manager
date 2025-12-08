using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

/// <summary>
/// Handler for indexing translations by setting InternalGroupName1 and InternalGroupName2 based on TranslationName patterns.
/// - Sets InternalGroupName1 to "Email" when TranslationName starts with "Email."
/// - Sets InternalGroupName2 to "EmailLayout" when TranslationName contains "Layout"
/// Processes translations in batches of 250 for efficient memory usage.
/// </summary>
public class IndexTranslationsCommandHandler 
    : IRequestHandler<IndexTranslationsCommand, IndexTranslationsResult>
{
    private readonly DataManagerDbContext _context;
    private readonly TranslationsQueryService _queryService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<IndexTranslationsCommandHandler> _logger;
    private const int BatchSize = 250;

    public IndexTranslationsCommandHandler(
        DataManagerDbContext context,
        TranslationsQueryService queryService,
        IAuthorizationService authorizationService,
        ILogger<IndexTranslationsCommandHandler> logger)
    {
        _context = context;
        _queryService = queryService;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<IndexTranslationsResult> Handle(
        IndexTranslationsCommand request, 
        CancellationToken cancellationToken)
    {
        var updatedCount = 0;
        var processedCount = 0;
        var errors = new List<string>();

        try
        {
            // Verify dataset access if DataSetId is provided
            if (request.DataSetId.HasValue)
            {
                var dataSetExists = await VerifyDataSetAccessAsync(request.DataSetId.Value, cancellationToken);
                if (!dataSetExists)
                {
                    errors.Add($"DataSet with ID {request.DataSetId} not found or not accessible.");
                    return new IndexTranslationsResult
                    {
                        UpdatedCount = 0,
                        ProcessedCount = 0,
                        Errors = errors
                    };
                }
            }

            // Process translations in batches
            var hasMore = true;
            var skip = 0;

            while (hasMore)
            {
                // Fetch a batch of translations
                var query = await _queryService.PrepareQueryAsync(
                    query: _context.Translations.AsQueryable(),
                    cancellationToken: cancellationToken);

                // Apply dataset filter if provided
                if (request.DataSetId.HasValue)
                {
                    query = query.Where(t => t.DataSetId == request.DataSetId.Value);
                }

                var batch = await query
                    .OrderBy(t => t.Id)
                    .Skip(skip)
                    .Take(BatchSize)
                    .ToListAsync(cancellationToken);

                if (!batch.Any())
                {
                    hasMore = false;
                    break;
                }

                processedCount += batch.Count;

                // Track updates in this batch
                var batchUpdatedCount = 0;

                // Process each translation in the batch
                foreach (var translation in batch)
                {
                    var originalGroupName1 = translation.InternalGroupName1;
                    var originalGroupName2 = translation.InternalGroupName2;
                    
                    // Apply indexing logic: Set InternalGroupName1 to "Email" if TranslationName starts with "Email."
                    if (!string.IsNullOrEmpty(translation.TranslationName) && 
                        translation.TranslationName.StartsWith("Email.", StringComparison.OrdinalIgnoreCase))
                    {
                        translation.InternalGroupName1 = "Email";
                        
                        // Additionally, if the same TranslationName also contains "Layout", 
                        // set InternalGroupName2 to "EmailLayout"
                        if (translation.TranslationName.Contains("Layout", StringComparison.OrdinalIgnoreCase))
                        {
                            translation.InternalGroupName2 = "EmailLayout";
                        }
                    }

                    // Track if the translation was updated
                    if (translation.InternalGroupName1 != originalGroupName1 || 
                        translation.InternalGroupName2 != originalGroupName2)
                    {
                        updatedCount++;
                        batchUpdatedCount++;
                    }
                }

                // Save changes for this batch
                await _context.SaveChangesAsync(cancellationToken);

                // Clear the ChangeTracker to release memory
                _context.ChangeTracker.Clear();

                _logger.LogInformation(
                    "Processed batch starting at {Skip}: {Count} translations, {Updated} updated in this batch",
                    skip, batch.Count, batchUpdatedCount);

                // Move to next batch
                skip += batch.Count;

                // Check if we've processed all translations
                if (batch.Count < BatchSize)
                {
                    hasMore = false;
                }
            }

            _logger.LogInformation(
                "Completed indexing translations. Processed: {ProcessedCount}, Updated: {UpdatedCount}",
                processedCount, updatedCount);

            return new IndexTranslationsResult
            {
                UpdatedCount = updatedCount,
                ProcessedCount = processedCount,
                Errors = errors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing translations");
            errors.Add($"Unexpected error: {ex.Message}");
            return new IndexTranslationsResult
            {
                UpdatedCount = updatedCount,
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
        var canAccess = await _authorizationService.CanAccessDataSetAsync(dataSetId, cancellationToken);
        return canAccess;
    }
}
