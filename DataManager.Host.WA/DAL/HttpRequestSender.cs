using System.Net;
using System.Text.Json;
using DataManager.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace DataManager.Host.WA.DAL;

public class HttpRequestSender : IRequestSender
{
    private readonly DataManagerHttpClient _httpClient;
    private readonly IConfiguration _configuration;
    
    private const string QuerySuffix = "Query";
    private const string CommandSuffix = "Command";

    public HttpRequestSender(
        DataManagerHttpClient httpClient,
        IConfiguration configuration
    )
    {
        // HttpClient wrapper that uses the named 'DataManager.API' client
        // which is configured with BaseAddressAuthorizationMessageHandler
        // that automatically attaches access tokens to API requests
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<TResponse> SendAsync<TResponse>(object request, CancellationToken cancellationToken = default)
    {
        var requestName = GetRequestName(request.GetType());
        var requestType = request.GetType();

        try
        {
            TResponse? data;

            // Use POST for MJML-related requests (potentially large HTML content)
            if (IsMjmlRequest(requestType))
            {
                data = await _httpClient.PostAsync<object, TResponse>($"query/{requestName}", request, cancellationToken);
            }
            else
            {
                // Use GET for all other requests (existing behavior)
                var requestAsJson = JsonSerializer.Serialize(request);
                var urlEncodedRequest = WebUtility.UrlEncode(requestAsJson);
                data = await _httpClient.GetAsync<TResponse>($"query/{requestName}?body={urlEncodedRequest}", cancellationToken);
            }

            return data!;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            if(_configuration.GetValue<bool>("Authentication:RequireAuthentication") == false)
            {
                throw;
            }
            else
            {
                // Redirect to login if access token is not available
                exception.Redirect();
                throw;
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Handle 401 Unauthorized - token might be expired or invalid
            throw new InvalidOperationException("Authentication failed. Please log in again.", ex);
        }
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>((object)request, cancellationToken);
    }

    /// <summary>
    /// Generates a request name for the given type, including generic type arguments
    /// Format: "GetTranslations&lt;SimpleTranslationDto&gt;" for generic types (suffix stripped)
    /// Strips "Query" and "Command" suffixes to make them transparent in the API
    /// </summary>
    private static string GetRequestName(Type requestType)
    {
        string baseTypeName;
        
        if (!requestType.IsGenericType)
        {
            baseTypeName = requestType.Name;
        }
        else
        {
            // For generic types, get the base name without the generic arity marker
            baseTypeName = requestType.Name;
            var backtickIndex = baseTypeName.IndexOf('`');
            if (backtickIndex > 0)
            {
                baseTypeName = baseTypeName.Substring(0, backtickIndex);
            }
        }

        // Strip "Query" or "Command" suffix from the base type name
        if (baseTypeName.EndsWith(QuerySuffix))
        {
            baseTypeName = baseTypeName.Substring(0, baseTypeName.Length - QuerySuffix.Length);
        }
        else if (baseTypeName.EndsWith(CommandSuffix))
        {
            baseTypeName = baseTypeName.Substring(0, baseTypeName.Length - CommandSuffix.Length);
        }

        // For generic types, append the generic arguments
        if (requestType.IsGenericType)
        {
            var genericArgs = requestType.GetGenericArguments();
            var argNames = string.Join(",", genericArgs.Select(t => t.Name));
            return $"{baseTypeName}<{argNames}>";
        }

        return baseTypeName;
    }

    /// <summary>
    /// Determines if the request is MJML-related (requires POST due to potentially large HTML content)
    /// </summary>
    private static bool IsMjmlRequest(Type requestType)
    {
        // Check if the request type is in the Mjml namespace (e.g., DataManager.Application.Contracts.Modules.Mjml)
        return requestType.Namespace?.EndsWith(".Mjml") == true || 
               requestType.Namespace?.Contains(".Mjml.") == true;
    }
}