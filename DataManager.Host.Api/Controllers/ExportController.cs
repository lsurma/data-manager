using System.Text.Json;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.Translations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataManager.Host.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/export")]
public class ExportController : ControllerBase
{
    private readonly ILogger<ExportController> _logger;
    private readonly IMediator _mediator;

    public ExportController(ILogger<ExportController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("translations")]
    public async Task<IActionResult> ExportTranslations([FromQuery] string? format, [FromQuery] string? orderBy, [FromQuery] string? orderDirection, [FromQuery] string? filtering)
    {
        _logger.LogInformation("Processing export translations request.");

        try
        {
            format ??= "csv";

            var query = new ExportTranslationsQuery
            {
                OrderBy = orderBy,
                OrderDirection = orderDirection,
                Format = format.ToLowerInvariant()
            };

            if (!string.IsNullOrEmpty(filtering))
            {
                query.Filtering = JsonSerializer.Deserialize<FilteringParameters>(filtering, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new QueryFilterJsonConverter() }
                });
            }

            var resultStream = await _mediator.Send(query);

            string contentType;
            string fileExtension;

            switch (format.ToLowerInvariant())
            {
                case "xlsx":
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileExtension = "xlsx";
                    break;
                default:
                    contentType = "text/csv";
                    fileExtension = "csv";
                    break;
            }

            return File(resultStream, contentType, $"translations_{DateTime.UtcNow:yyyyMMddHHmmss}.{fileExtension}");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize filters for export translations request.");
            return BadRequest(new { error = "Invalid JSON in filters parameter.", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing export translations request.");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
