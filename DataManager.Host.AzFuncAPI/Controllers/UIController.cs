using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DataManager.Host.AzFuncAPI.Controllers;

/// <summary>
/// Controller that serves static files from wwwroot and provides SPA fallback.
/// Uses a wildcard route to catch all non-API requests and serve static files
/// or fall back to index.html for client-side routing.
/// </summary>
public class UIController
{
    private readonly ILogger<UIController> _logger;
    private static readonly string WwwrootPath = GetWwwrootPath();
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    public UIController(ILogger<UIController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Serves static files or falls back to index.html for client-side routing.
    /// This is the catch-all route for all non-API GET requests, including the root path.
    /// The {*path} pattern captures the entire URL path, including empty string for root.
    /// </summary>
    [Function("UI")]
    public async Task<IActionResult> ServeUI(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ui/{*path}")] HttpRequest req,
        string? path)
    {
        path ??= string.Empty;
        
        // Skip API routes - they should be handled by other controllers
        if (path.StartsWith("api/", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("api", StringComparison.OrdinalIgnoreCase))
        {
            return new NotFoundResult();
        }

        return await ServeStaticFileOrFallback(path);
    }

    private async Task<IActionResult> ServeStaticFileOrFallback(string relativePath)
    {
        // Try to serve a static file first
        if (!string.IsNullOrEmpty(relativePath))
        {
            var staticFilePath = Path.Combine(WwwrootPath, relativePath);
            var fullPath = Path.GetFullPath(staticFilePath);
            var wwwrootFullPath = Path.GetFullPath(WwwrootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            // Security check: ensure the path is within wwwroot
            if (fullPath.StartsWith(wwwrootFullPath, StringComparison.Ordinal) && File.Exists(fullPath))
            {
                var result = await TryServeFileAsync(fullPath, relativePath);
                if (result != null)
                {
                    return result;
                }
            }
        }

        // SPA fallback: serve index.html for client-side routing
        var indexPath = Path.Combine(WwwrootPath, "index.html");
        if (File.Exists(indexPath))
        {
            _logger.LogDebug("SPA fallback: serving index.html for path {Path}", relativePath);
            var result = await TryServeFileAsync(indexPath, "index.html");
            if (result != null)
            {
                return result;
            }
        }

        _logger.LogWarning("No static file found and no index.html available. WwwrootPath: {WwwrootPath}", WwwrootPath);
        return new NotFoundObjectResult(new { error = "Static file not found", requestedPath = relativePath });
    }

    private async Task<IActionResult?> TryServeFileAsync(string fullPath, string relativePath)
    {
        try
        {
            var contentType = GetContentType(relativePath);
            _logger.LogDebug("Serving static file: {Path} with content type: {ContentType}", relativePath, contentType);

            var fileBytes = await File.ReadAllBytesAsync(fullPath);
            return new FileContentResult(fileBytes, contentType);
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Failed to serve file: {Path}", relativePath);
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied when serving file: {Path}", relativePath);
            return null;
        }
    }

    private static string GetContentType(string path)
    {
        if (ContentTypeProvider.TryGetContentType(path, out var contentType))
        {
            return contentType;
        }

        // Default content type for unknown file types
        return "application/octet-stream";
    }

    private static string GetWwwrootPath()
    {
        // The wwwroot folder is copied to the output directory during build
        var assemblyLocation = Path.GetDirectoryName(typeof(UIController).Assembly.Location);
        return Path.GetFullPath(Path.Combine(assemblyLocation ?? AppContext.BaseDirectory, "wwwroot"));
    }
}
