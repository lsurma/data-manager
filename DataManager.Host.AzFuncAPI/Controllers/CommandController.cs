using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DataManager.Application.Contracts;
using DataManager.Host.AzFuncAPI.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DataManager.Host.AzFuncAPI.Controllers;

[Authorize]
public class CommandController
{
    private const int MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB limit
    private readonly ILogger<CommandController> _logger;
    private readonly IMediator _mediator;
    private readonly RequestRegistry _requestRegistry;

    public CommandController(ILogger<CommandController> logger, IMediator mediator, RequestRegistry requestRegistry)
    {
        _logger = logger;
        _mediator = mediator;
        _requestRegistry = requestRegistry;
    }

    /// <summary>
    /// Main command endpoint that routes MediatR commands.
    /// Authentication is handled by the authorization middleware.
    /// Supports both JWT Bearer tokens (Authorization: Bearer {token}) and API Keys (X-API-Key: {key}).
    /// Accepts POST, PUT, and DELETE methods with body in request body.
    /// </summary>
    [Function("Command")]
    public async Task<IActionResult> Command(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", "delete", Route = "api/command/{requestName}")] HttpRequest req,
        string requestName
    )
    {
        if (req.ContentLength > MaxRequestBodySize)
        {
            return new BadRequestObjectResult(new { error = $"Request body exceeds maximum size of {MaxRequestBodySize / (1024 * 1024)} MB." });
        }

        using var reader = new StreamReader(req.Body, Encoding.UTF8);
        var bodyJson = await reader.ReadToEndAsync();

        if (bodyJson.Length > MaxRequestBodySize)
        {
            return new BadRequestObjectResult(new { error = $"Request body exceeds maximum size of {MaxRequestBodySize / (1024 * 1024)} MB." });
        }

        if (string.IsNullOrWhiteSpace(requestName))
        {
            return new BadRequestObjectResult(new { error = "Request name is required." });
        }

        _logger.LogInformation("Processing command: {RequestName}", requestName);

        try
        {
            var requestType = _requestRegistry.GetRequestType(requestName);
            if (requestType == null)
            {
                return new NotFoundObjectResult(new
                {
                    error = $"Command '{requestName}' not found.",
                    availableRequests = _requestRegistry.GetAllRequestNames()
                });
            }

            // Verify this is actually a command type
            if (!_requestRegistry.IsCommandType(requestType))
            {
                return new BadRequestObjectResult(new 
                { 
                    error = $"'{requestName}' is not a command. Commands should use the /api/command endpoint. Queries should use the /api/query endpoint." 
                });
            }

            // Deserialize body if provided, otherwise create empty instance
            object? request;
            if (!string.IsNullOrWhiteSpace(bodyJson))
            {
                request = JsonSerializer.Deserialize(bodyJson, requestType, DataManagerJsonSerializerOptions.Default);
            }
            else
            {
                request = Activator.CreateInstance(requestType);
            }

            if (request == null)
            {
                return new BadRequestObjectResult(new { error = "Failed to create request instance." });
            }

            // Send through MediatR
            var result = await _mediator.Send(request);

            return new JsonResult(result, DataManagerJsonSerializerOptions.Default);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize request body for: {RequestName}", requestName);
            return new BadRequestObjectResult(new { error = "Invalid JSON in body parameter.", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command: {RequestName}", requestName);
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

}
