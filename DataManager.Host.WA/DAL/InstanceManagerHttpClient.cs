using System.Net.Http.Json;
using System.Text.Json;
using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Common;

namespace DataManager.Host.WA.DAL;

/// <summary>
/// Typed HttpClient for DataManager API.
/// This client automatically includes access tokens for authenticated requests.
/// Configured with BaseAddressAuthorizationMessageHandler.
/// </summary>
public class DataManagerHttpClient
{
    public HttpClient Client { get; }

    public DataManagerHttpClient(HttpClient httpClient)
    {
        Client = httpClient;
    }
    
    public async Task<TResponse?> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default)
    {
        var response = await Client.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiErrorExceptionAsync(response, cancellationToken);
        }
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest content, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync(requestUri, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiErrorExceptionAsync(response, cancellationToken);
        }
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    public async Task<HttpResponseMessage> PostAsync<TRequest>(string requestUri, TRequest content, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync(requestUri, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiErrorExceptionAsync(response, cancellationToken);
        }
        return response;
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string requestUri, TRequest content, CancellationToken cancellationToken = default)
    {
        var response = await Client.PutAsJsonAsync(requestUri, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiErrorExceptionAsync(response, cancellationToken);
        }
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    public async Task<HttpResponseMessage> PutAsync<TRequest>(string requestUri, TRequest content, CancellationToken cancellationToken = default)
    {
        var response = await Client.PutAsJsonAsync(requestUri, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiErrorExceptionAsync(response, cancellationToken);
        }
        return response;
    }

    public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        var response = await Client.DeleteAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiErrorExceptionAsync(response, cancellationToken);
        }
        return response;
    }

    public async Task<TResponse?> DeleteAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default)
    {
        var response = await Client.DeleteAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiErrorExceptionAsync(response, cancellationToken);
        }
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    /// <summary>
    /// Downloads a file from the API and returns it with metadata (content type, filename)
    /// </summary>
    public async Task<DownloadedFile> GetFileAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        var response = await Client.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiErrorExceptionAsync(response, cancellationToken);
        }

        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        string? fileName = null;

        // Try to get filename from Content-Disposition header
        if (response.Content.Headers.ContentDisposition?.FileNameStar != null)
        {
            fileName = response.Content.Headers.ContentDisposition.FileNameStar;
        }
        else if (response.Content.Headers.ContentDisposition?.FileName != null)
        {
            fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');
        }

        return new DownloadedFile
        {
            Content = content,
            ContentType = contentType,
            FileName = fileName
        };
    }

    /// <summary>
    /// Reads the error response from the API and throws an ApiErrorException with detailed error information.
    /// </summary>
    private static async Task ThrowApiErrorExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var statusCode = (int)response.StatusCode;
        string errorMessage = $"HTTP {statusCode} error";
        string? errorDetails = null;

        try
        {
            // Try to read the error response body
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                // Try to deserialize as ApiErrorResponse
                var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(responseBody, DataManagerJsonSerializerOptions.Default);

                if (errorResponse != null && !string.IsNullOrWhiteSpace(errorResponse.Error))
                {
                    errorMessage = errorResponse.Error;
                    errorDetails = errorResponse.Details;
                }
                else
                {
                    // If we can't parse as ApiErrorResponse, use the raw response body
                    errorDetails = responseBody;
                }
            }
        }
        catch
        {
            // If we fail to read the response body, fall back to the default error message
        }

        throw new ApiErrorException(errorMessage, errorDetails, statusCode);
    }
}

