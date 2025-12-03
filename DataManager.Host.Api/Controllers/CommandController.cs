using System.Text.Json;
using System.Text.Json.Serialization;
using DataManager.Host.Shared.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataManager.Host.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/command")]
public class CommandController : ControllerBase
{
    private readonly ILogger<CommandController> _logger;
    private readonly IMediator _mediator;
    private readonly RequestRegistry _requestRegistry;

    public CommandController(ILogger<CommandController> logger, IMediator mediator, RequestRegistry requestRegistry)
    {
        _logger = logger;
        _mediator = mediator;
        _requestRegistry = requestRegistry;
    }

    [HttpPost("{requestName}")]
    [HttpPut("{requestName}")]
    [HttpDelete("{requestName}")]
    public async Task<IActionResult> Command(string requestName, [FromBody] JsonElement body)
    {
        if (string.IsNullOrWhiteSpace(requestName))
        {
            return BadRequest(new { error = "Request name is required." });
        }

        _logger.LogInformation("Processing command: {RequestName}", requestName);

        try
        {
            var requestType = _requestRegistry.GetRequestType(requestName);
            if (requestType == null)
            {
                return NotFound(new
                {
                    error = $"Command '{requestName}' not found.",
                    availableRequests = _requestRegistry.GetAllRequestNames()
                });
            }

            if (!_requestRegistry.IsCommandType(requestType))
            {
                return BadRequest(new { error = $"'{requestName}' is not a command." });
            }

            object? request;
            if (body.ValueKind != JsonValueKind.Undefined)
            {
                request = JsonSerializer.Deserialize(body.GetRawText(), requestType, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                request = Activator.CreateInstance(requestType);
            }

            if (request == null)
            {
                return BadRequest(new { error = "Failed to create request instance." });
            }

            var result = await _mediator.Send(request);

            return new JsonResult(result, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize request body for: {RequestName}", requestName);
            return BadRequest(new { error = "Invalid JSON in body.", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command: {RequestName}", requestName);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
