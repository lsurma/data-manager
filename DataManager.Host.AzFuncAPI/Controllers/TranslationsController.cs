using System.Text.Json;
using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.Translations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DataManager.Application.Core.Data;
using DataManager.Application.Core.Modules.DataSets;

namespace DataManager.Host.AzFuncAPI.Controllers;

[Authorize]
public class TranslationsController
{
    private readonly ILogger<TranslationsController> _logger;
    private readonly IMediator _mediator;
    private readonly DataManagerDbContext _context;

    public TranslationsController(ILogger<TranslationsController> logger, IMediator mediator, DataManagerDbContext context)
    {
        _logger = logger;
        _mediator = mediator;
        _context = context;
    }

    /// <summary>
    /// Get translations for a specific dataset by name or ID
    /// Returns simplified translation data with only essential fields
    /// Supports query parameters: limit, offset, specificDataSetId, cultures
    /// </summary>
    [Function("GetTranslations")]
    public async Task<IActionResult> GetTranslations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/translations/{translationSetNameOrId}")] HttpRequest req,
        string translationSetNameOrId)
    {
        _logger.LogInformation("Getting translations for translationsset: {TranslationSetNameOrId}", translationSetNameOrId);

        try
        {
            // Resolve dataset by name or ID
            var dataSet = await ResolveTranslationSetAsync(translationSetNameOrId);
            if (dataSet == null)
            {
                return new NotFoundObjectResult(new { error = $"DataSet '{translationSetNameOrId}' not found." });
            }

            // Parse query parameters
            var limit = int.TryParse(req.Query["limit"], out var limitValue) ? limitValue : 20;
            var offset = int.TryParse(req.Query["offset"], out var offsetValue) ? offsetValue : 0;
            var specificDataSetIdStr = req.Query["specificDataSetId"].ToString();
            var culturesStr = req.Query["cultures"].ToString();
            var contentUpdatedAtAfter = req.Query["contentUpdatedAtAfter"].ToString();
            DateTimeOffset? contentUpdatedAtAfterAsDateTimeOffset = null;
            
            if (!string.IsNullOrWhiteSpace(contentUpdatedAtAfter) && DateTimeOffset.TryParse(contentUpdatedAtAfter, out DateTimeOffset parsedDateTimeOffset))
            {
                contentUpdatedAtAfterAsDateTimeOffset = parsedDateTimeOffset;
            }

            // Parse cultures (comma-separated)
            var cultures = !string.IsNullOrWhiteSpace(culturesStr)
                ? culturesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : Array.Empty<string>();

            // Resolve specific translation set if provided
            Guid? specificDataSetId = null;
            if (!string.IsNullOrWhiteSpace(specificDataSetIdStr))
            {
                var specificTranslationSet = await ResolveTranslationSetAsync(specificDataSetIdStr);
                if (specificTranslationSet != null)
                {
                    specificDataSetId = specificTranslationSet.Id;
                }
            }

            // Create simplified export query
            var query = new GetTranslationsForExportQuery
            {
                DataSetId = dataSet.Id,
                SpecificDataSetId = specificDataSetId,
                Cultures = cultures,
                Limit = limit,
                Offset = offset,
                ContentUpdatedAtAfter = contentUpdatedAtAfterAsDateTimeOffset
            };

            var result = await _mediator.Send(query);

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting translations for translationsset: {TranslationSetNameOrId}", translationSetNameOrId);
            return new ObjectResult(new { error = ex.Message }) { StatusCode = 500 };
        }
    }

    /// <summary>
    /// Import translations from a remote source for a specific dataset
    /// Accepts a JSON list of translation objects
    /// </summary>
    [Function("ImportTranslations")]
    public async Task<IActionResult> ImportTranslations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/translations/{translationSetNameOrId}")] HttpRequest req,
        string translationSetNameOrId)
    {
        _logger.LogInformation("Importing translations for translationsset: {TranslationSetNameOrId}", translationSetNameOrId);

        try
        {
            // Resolve dataset by name or ID
            var dataSet = await ResolveTranslationSetAsync(translationSetNameOrId);
            if (dataSet == null)
            {
                return new NotFoundObjectResult(new { error = $"DataSet '{translationSetNameOrId}' not found." });
            }

            // Read request body as JSON
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return new BadRequestObjectResult(new { error = "Request body is empty." });
            }

            var translations = JsonSerializer.Deserialize<List<ImportTranslationInput>>(requestBody, DataManagerJsonSerializerOptions.Default);

            if (translations == null || translations.Count == 0)
            {
                return new BadRequestObjectResult(new { error = "No translations provided." });
            }

            // Use the ImportTranslationsCommand
            var command = new ImportTranslationsCommand
            {
                DataSetId = dataSet.Id,
                Translations = translations
            };

            var result = await _mediator.Send(command);

            return new OkObjectResult(new 
            { 
                message = "Translations import completed.", 
                translationSetId = dataSet.Id,
                importedCount = result.ImportedCount,
                failedCount = result.FailedCount,
                errors = result.Errors
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize request body for translationsset: {TranslationSetNameOrId}", translationSetNameOrId);
            return new BadRequestObjectResult(new { error = "Invalid JSON format.", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing translations for translationsset: {TranslationSetNameOrId}", translationSetNameOrId);
            return new ObjectResult(new { error = ex.Message }) { StatusCode = 500 };
        }
    }

    /// <summary>
    /// Helper method to resolve a dataset by name or ID
    /// </summary>
    private async Task<DataSet?> ResolveTranslationSetAsync(string nameOrId)
    {
        // Try parsing as Guid first
        if (Guid.TryParse(nameOrId, out var translationSetId))
        {
            return await _context.DataSets
                .AsNoTracking()
                .FirstOrDefaultAsync(ds => ds.Id == translationSetId);
        }

        // Otherwise treat as name
        return await _context.DataSets
            .AsNoTracking()
            .FirstOrDefaultAsync(ds => ds.Name == nameOrId);
    }
}
