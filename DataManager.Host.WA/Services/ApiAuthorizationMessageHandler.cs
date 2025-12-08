using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Logging;

namespace DataManager.Host.WA.Services;

/// <summary>
/// A custom <see cref="AuthorizationMessageHandler"/> that attaches access tokens to outgoing HTTP requests
/// for external API endpoints (not limited to the application's base URI).
/// When no access token is available (user not logged in), the request is passed through without
/// authentication, allowing API endpoints that don't require authorization to still work.
/// </summary>
public class ApiAuthorizationMessageHandler : AuthorizationMessageHandler
{
    private readonly ILogger<ApiAuthorizationMessageHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ApiAuthorizationMessageHandler"/>.
    /// </summary>
    /// <param name="provider">The <see cref="IAccessTokenProvider"/> to use for requesting tokens.</param>
    /// <param name="navigationManager">The <see cref="NavigationManager"/>.</param>
    /// <param name="logger">The logger for logging errors.</param>
    /// <param name="apiBaseUrl">The base URL of the API that should receive authorization tokens.</param>
    /// <param name="scopes">Optional scopes to request for the access token.</param>
    public ApiAuthorizationMessageHandler(
        IAccessTokenProvider provider, 
        NavigationManager navigationManager,
        ILogger<ApiAuthorizationMessageHandler> logger,
        string apiBaseUrl,
        string[]? scopes = null)
        : base(provider, navigationManager)
    {
        _logger = logger;
        ConfigureHandler(
            authorizedUrls: new[] { apiBaseUrl, "https://graph.microsoft.com" },
            scopes: scopes ?? Array.Empty<string>()
        );
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        catch (AccessTokenNotAvailableException ex)
        {
            // Log the error and pass the request through without authentication
            // This allows API endpoints that don't require authorization to still work
            _logger.LogDebug(ex, "Access token not available for {RequestUri}, passing request without authentication", request.RequestUri);
            
            // Send the request without the authorization header using the inner handler
            // InnerHandler is always set when this handler is used in a pipeline (e.g., via AddHttpMessageHandler)
            using var invoker = new HttpMessageInvoker(InnerHandler!, disposeHandler: false);
            return await invoker.SendAsync(request, cancellationToken);
        }
    }
}

