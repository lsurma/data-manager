using System.Text.Json;
using DataManager.Application.Contracts.Modules.DataSet;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataManager.Host.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/import")]
public class ImportController : ControllerBase
{
    private readonly ILogger<ImportController> _logger;
    private readonly IMediator _mediator;

    public ImportController(ILogger<ImportController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpPost("translations")]
    public async Task<IActionResult> UploadTranslationFile([FromForm] IFormFile file, [FromForm] Guid dataSetId)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file received.");
            }

            var command = new UploadTranslationFileCommand
            {
                DataSetId = dataSetId,
                FileName = file.FileName,
                Content = file.OpenReadStream()
            };

            await _mediator.Send(command);

            return Ok(new { message = "File uploaded successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading translation file.");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("translations/process")]
    public async Task<IActionResult> ProcessTranslationFile([FromBody] ProcessTranslationFileCommand command)
    {
        try
        {
            await _mediator.Send(command);

            return Ok(new { message = "File processed successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing translation file.");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("translations/{dataSetId}")]
    public async Task<IActionResult> GetUploadedFiles(Guid dataSetId)
    {
        try
        {
            var query = new GetUploadedFilesQuery { DataSetId = dataSetId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting uploaded files.");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
