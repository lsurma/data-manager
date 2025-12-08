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
public class QueryController
{
    private const int MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB limit
    private readonly ILogger<QueryController> _logger;
    private readonly IMediator _mediator;
    private readonly RequestRegistry _requestRegistry;

    public QueryController(ILogger<QueryController> logger, IMediator mediator, RequestRegistry requestRegistry)
    {
        _logger = logger;
        _mediator = mediator;
        _requestRegistry = requestRegistry;
    }

    /// <summary>
    /// Main query endpoint that routes MediatR queries.
    /// Authentication is handled by the authorization middleware.
    /// Supports both JWT Bearer tokens (Authorization: Bearer {token}) and API Keys (X-API-Key: {key}).
    /// Accepts only GET method (with body in query parameter).
    /// </summary>
    [Function("Query")]
    public async Task<IActionResult> Query(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/query/{requestName}")] HttpRequest req,
        string requestName
    )
    {
        var bodyJson = req.Query["body"].ToString();

        if (string.IsNullOrWhiteSpace(requestName))
        {
            return new BadRequestObjectResult(new { error = "Request name is required." });
        }

        _logger.LogInformation("Processing query: {RequestName}", requestName);

        try
        {
            var requestType = _requestRegistry.GetRequestType(requestName);
            if (requestType == null)
            {
                return new NotFoundObjectResult(new
                {
                    error = $"Query '{requestName}' not found.",
                    availableRequests = _requestRegistry.GetAllRequestNames()
                });
            }

            // Verify this is actually a query type
            if (!_requestRegistry.IsQueryType(requestType))
            {
                return new BadRequestObjectResult(new 
                { 
                    error = $"'{requestName}' is not a query. Queries should use the /api/query endpoint. Commands should use the /api/command endpoint." 
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

            // Check if result is a Stream (file download)
            if (result is Stream stream)
            {
                return CreateFileStreamResult(stream, requestType, request);
            }

            return new JsonResult(result, DataManagerJsonSerializerOptions.Default);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize request body for: {RequestName}", requestName);
            return new BadRequestObjectResult(new { error = "Invalid JSON in body parameter.", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query: {RequestName}", requestName);
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a FileStreamResult from a Stream, determining the content type and filename based on the request.
    /// </summary>
    private FileStreamResult CreateFileStreamResult(Stream stream, Type requestType, object request)
    {
        string contentType = "application/octet-stream";
        string fileExtension = "bin";
        string fileName = $"export_{DateTime.UtcNow:yyyyMMddHHmmss}";

        // Try to get format from request if it has a Format property
        var formatProperty = requestType.GetProperty("Format");
        if (formatProperty != null)
        {
            var format = formatProperty.GetValue(request)?.ToString()?.ToLowerInvariant() ?? "csv";

            switch (format)
            {
                case "xlsx":
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileExtension = "xlsx";
                    fileName = "translations";
                    break;
                case "csv":
                    contentType = "text/csv";
                    fileExtension = "csv";
                    fileName = "translations";
                    break;
            }
        }

        return new FileStreamResult(stream, contentType)
        {
            FileDownloadName = $"{fileName}_{DateTime.UtcNow:yyyyMMddHHmmss}.{fileExtension}"
        };
    }

}