using System.Text.Json;
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
    /// Supports query parameters: orderBy, orderDirection, limit, offset
    /// </summary>
    [Function("GetTranslations")]
    public async Task<IActionResult> GetTranslations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/translations/{dataSetNameOrId}")] HttpRequest req,
        string dataSetNameOrId)
    {
        _logger.LogInformation("Getting translations for dataset: {DataSetNameOrId}", dataSetNameOrId);

        try
        {
            // Resolve dataset by name or ID
            var dataSet = await ResolveDataSetAsync(dataSetNameOrId);
            if (dataSet == null)
            {
                return new NotFoundObjectResult(new { error = $"DataSet '{dataSetNameOrId}' not found." });
            }

            // Parse query parameters
            var orderBy = req.Query["orderBy"].ToString();
            var orderDirection = req.Query["orderDirection"].ToString();
            var limit = int.TryParse(req.Query["limit"], out var limitValue) ? limitValue : 20;
            var offset = int.TryParse(req.Query["offset"], out var offsetValue) ? offsetValue : 0;

            // Create query with simplified DTO projection
            var query = new GetTranslationsQuery<SimpleTranslationDto>
            {
                Pagination = new PaginationParameters
                {
                    Skip = offset,
                    PageSize = limit
                },
                Ordering = new OrderingParameters
                {
                    OrderBy = orderBy,
                    OrderDirection = orderDirection
                },
                Filtering = new FilteringParameters
                {
                    QueryFilters = new List<IQueryFilter>
                    {
                        new DataSetIdFilter { Value = dataSet.Id }
                    }
                }
            };

            var result = await _mediator.Send(query);

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting translations for dataset: {DataSetNameOrId}", dataSetNameOrId);
            return new ObjectResult(new { error = ex.Message }) { StatusCode = 500 };
        }
    }

    /// <summary>
    /// Import translations from a remote source for a specific dataset
    /// Accepts a JSON list of translation objects
    /// </summary>
    [Function("ImportTranslations")]
    public async Task<IActionResult> ImportTranslations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/translations/{dataSetNameOrId}")] HttpRequest req,
        string dataSetNameOrId)
    {
        _logger.LogInformation("Importing translations for dataset: {DataSetNameOrId}", dataSetNameOrId);

        try
        {
            // Resolve dataset by name or ID
            var dataSet = await ResolveDataSetAsync(dataSetNameOrId);
            if (dataSet == null)
            {
                return new NotFoundObjectResult(new { error = $"DataSet '{dataSetNameOrId}' not found." });
            }

            // Read request body as JSON
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return new BadRequestObjectResult(new { error = "Request body is empty." });
            }

            var translations = JsonSerializer.Deserialize<List<ImportTranslationDto>>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

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
                dataSetId = dataSet.Id,
                importedCount = result.ImportedCount,
                failedCount = result.FailedCount,
                errors = result.Errors
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize request body for dataset: {DataSetNameOrId}", dataSetNameOrId);
            return new BadRequestObjectResult(new { error = "Invalid JSON format.", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing translations for dataset: {DataSetNameOrId}", dataSetNameOrId);
            return new ObjectResult(new { error = ex.Message }) { StatusCode = 500 };
        }
    }

    /// <summary>
    /// Helper method to resolve a dataset by name or ID
    /// </summary>
    private async Task<Application.Core.Modules.DataSet.DataSet?> ResolveDataSetAsync(string nameOrId)
    {
        // Try parsing as Guid first
        if (Guid.TryParse(nameOrId, out var dataSetId))
        {
            return await _context.DataSets
                .AsNoTracking()
                .FirstOrDefaultAsync(ds => ds.Id == dataSetId);
        }

        // Otherwise treat as name
        return await _context.DataSets
            .AsNoTracking()
            .FirstOrDefaultAsync(ds => ds.Name == nameOrId);
    }
}
