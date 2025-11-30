using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Logging;

namespace DataManager.Host.WA.Services;

/// <summary>
/// A custom <see cref="AuthorizationMessageHandler"/> that attaches access tokens to outgoing HTTP requests
/// for external API endpoints (not limited to the application's base URI).
/// When no access token is available (user not logged in), the exception is caught and logged,
/// and an empty unsuccessful response is returned instead of throwing.
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
            // Log the error and return an unauthorized response when user is not logged in
            _logger.LogDebug(ex, "Access token not available for {RequestUri}, returning Unauthorized response", request.RequestUri);
            
            return new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                RequestMessage = request,
                ReasonPhrase = "Access token not available"
            };
        }
    }
}

