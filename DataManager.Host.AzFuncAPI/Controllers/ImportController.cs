using System.Text.Json;
using DataManager.Application.Contracts.Modules.DataSet;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DataManager.Host.AzFuncAPI.Controllers;

public class ImportController
{
    private readonly ILogger<ImportController> _logger;
    private readonly IMediator _mediator;

    public ImportController(ILogger<ImportController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [Function("UploadTranslationFile")]
    public async Task<IActionResult> UploadTranslationFile(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/import/translations")] HttpRequest req)
    {
        try
        {
            var formdata = await req.ReadFormAsync();
            var file = req.Form.Files["file"];

            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("No file received.");
            }

            var dataSetId = Guid.Parse(req.Form["dataSetId"]);
            var command = new UploadTranslationFileCommand
            {
                DataSetId = dataSetId,
                FileName = file.FileName,
                Content = file.OpenReadStream()
            };

            await _mediator.Send(command);

            return new OkObjectResult(new { message = "File uploaded successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading translation file.");
            return new ObjectResult(new { error = ex.Message }) { StatusCode = 500 };
        }
    }

    [Function("ProcessTranslationFile")]
    public async Task<IActionResult> ProcessTranslationFile(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/import/translations/process")] HttpRequest req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var command = JsonSerializer.Deserialize<ProcessTranslationFileCommand>(requestBody);

            await _mediator.Send(command);

            return new OkObjectResult(new { message = "File processed successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing translation file.");
            return new ObjectResult(new { error = ex.Message }) { StatusCode = 500 };
        }
    }

    [Function("GetUploadedFiles")]
    public async Task<IActionResult> GetUploadedFiles(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/import/translations/{dataSetId}")] HttpRequest req, Guid dataSetId)
    {
        try
        {
            var query = new GetUploadedFilesQuery { DataSetId = dataSetId };
            var result = await _mediator.Send(query);
            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting uploaded files.");
            return new ObjectResult(new { error = ex.Message }) { StatusCode = 500 };
        }
    }
}