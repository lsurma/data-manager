using System.Text.Json;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.Translations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataManager.Application.Contracts.Modules.DataSet;

namespace DataManager.Host.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/translations")]
public class TranslationsController : ControllerBase
{
    private readonly ILogger<TranslationsController> _logger;
    private readonly IMediator _mediator;

    public TranslationsController(ILogger<TranslationsController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("{dataSetNameOrId}")]
    public async Task<IActionResult> GetTranslations(string dataSetNameOrId, [FromQuery] string? orderBy, [FromQuery] string? orderDirection, [FromQuery] int limit = 20, [FromQuery] int offset = 0)
    {
        _logger.LogInformation("Getting translations for dataset: {DataSetNameOrId}", dataSetNameOrId);

        try
        {
            var dataSet = await _mediator.Send(new ResolveDataSetQuery { NameOrId = dataSetNameOrId });
            if (dataSet == null)
            {
                return NotFound(new { error = $"DataSet '{dataSetNameOrId}' not found." });
            }

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

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting translations for dataset: {DataSetNameOrId}", dataSetNameOrId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{dataSetNameOrId}")]
    public async Task<IActionResult> ImportTranslations(string dataSetNameOrId, [FromBody] List<ImportTranslationDto> translations)
    {
        _logger.LogInformation("Importing translations for dataset: {DataSetNameOrId}", dataSetNameOrId);

        try
        {
            var dataSet = await _mediator.Send(new ResolveDataSetQuery { NameOrId = dataSetNameOrId });
            if (dataSet == null)
            {
                return NotFound(new { error = $"DataSet '{dataSetNameOrId}' not found." });
            }

            if (translations == null || translations.Count == 0)
            {
                return BadRequest(new { error = "No translations provided." });
            }

            var command = new ImportTranslationsCommand
            {
                DataSetId = dataSet.Id,
                Translations = translations
            };

            var result = await _mediator.Send(command);

            return Ok(new
            {
                message = "Translations import completed.",
                dataSetId = dataSet.Id,
                importedCount = result.ImportedCount,
                failedCount = result.FailedCount,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing translations for dataset: {DataSetNameOrId}", dataSetNameOrId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

}
