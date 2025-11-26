using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace InstanceManager.Host.AzFuncAPI.Controllers;

public class UIController
{
    private readonly ILogger<UIController> _logger;
    private static readonly string WwwrootPath = GetWwwrootPath();

    public UIController(ILogger<UIController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Serves the index.html file from the wwwroot directory.
    /// This endpoint is the entry point for the UI host.
    /// </summary>
    [Function("UI")]
    public IActionResult ServeIndex(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ui")] HttpRequest req)
    {
        _logger.LogInformation("Serving index.html");

        var indexPath = Path.Combine(WwwrootPath, "index.html");
        
        // Validate that the resolved path is within the wwwroot directory
        var fullPath = Path.GetFullPath(indexPath);
        if (!fullPath.StartsWith(Path.GetFullPath(WwwrootPath), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Attempted to access file outside of wwwroot: {Path}", fullPath);
            return new NotFoundObjectResult(new { error = "File not found" });
        }

        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("index.html not found at {Path}", fullPath);
            return new NotFoundObjectResult(new { error = "index.html not found" });
        }

        return new PhysicalFileResult(fullPath, "text/html");
    }

    private static string GetWwwrootPath()
    {
        // The wwwroot folder is copied to the output directory during build
        var assemblyLocation = Path.GetDirectoryName(typeof(UIController).Assembly.Location);
        return Path.GetFullPath(Path.Combine(assemblyLocation ?? AppContext.BaseDirectory, "wwwroot"));
    }
}
