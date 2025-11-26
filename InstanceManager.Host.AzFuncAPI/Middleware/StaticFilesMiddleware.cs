using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace InstanceManager.Host.AzFuncAPI.Middleware;

/// <summary>
/// Middleware that serves static files from wwwroot and provides SPA fallback.
/// Handles all GET requests that don't start with /api/ by serving static files
/// or falling back to index.html for client-side routing.
/// </summary>
public class StaticFilesMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<StaticFilesMiddleware> _logger;
    private static readonly string WwwrootPath = GetWwwrootPath();
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    public StaticFilesMiddleware(ILogger<StaticFilesMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext == null)
        {
            // Not an HTTP request, continue to next middleware
            await next(context);
            return;
        }

        var request = httpContext.Request;
        var path = request.Path.Value ?? string.Empty;

        // Only handle GET requests
        if (!HttpMethods.IsGet(request.Method))
        {
            await next(context);
            return;
        }

        // Skip API routes - let them be handled by function endpoints
        if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/api", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // Try to serve a static file
        var relativePath = path.TrimStart('/');

        if (!string.IsNullOrEmpty(relativePath))
        {
            var staticFilePath = Path.Combine(WwwrootPath, relativePath);
            var fullPath = Path.GetFullPath(staticFilePath);
            var wwwrootFullPath = Path.GetFullPath(WwwrootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            // Security check: ensure the path is within wwwroot
            // Normalize wwwrootFullPath to always end with separator for consistent comparison
            if (fullPath.StartsWith(wwwrootFullPath, StringComparison.Ordinal) && File.Exists(fullPath))
            {
                if (await TryServeFileAsync(httpContext, fullPath, relativePath))
                {
                    return;
                }
            }
        }

        // SPA fallback: serve index.html for client-side routing
        var indexPath = Path.Combine(WwwrootPath, "index.html");
        if (File.Exists(indexPath))
        {
            _logger.LogDebug("SPA fallback: serving index.html for path {Path}", path);
            if (await TryServeFileAsync(httpContext, indexPath, "index.html"))
            {
                return;
            }
        }

        // No static file found and no index.html, continue to next middleware
        await next(context);
    }

    private async Task<bool> TryServeFileAsync(HttpContext httpContext, string fullPath, string relativePath)
    {
        try
        {
            var contentType = GetContentType(relativePath);
            _logger.LogDebug("Serving static file: {Path} with content type: {ContentType}", relativePath, contentType);

            httpContext.Response.ContentType = contentType;
            httpContext.Response.StatusCode = StatusCodes.Status200OK;

            await using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await fileStream.CopyToAsync(httpContext.Response.Body);
            return true;
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Failed to serve file: {Path}", relativePath);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied when serving file: {Path}", relativePath);
            return false;
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
        var assemblyLocation = Path.GetDirectoryName(typeof(StaticFilesMiddleware).Assembly.Location);
        return Path.GetFullPath(Path.Combine(assemblyLocation ?? AppContext.BaseDirectory, "wwwroot"));
    }
}
