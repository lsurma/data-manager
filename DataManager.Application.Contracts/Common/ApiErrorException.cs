namespace DataManager.Application.Contracts.Common;

/// <summary>
/// Exception thrown when an API request returns an error response with detailed error information.
/// </summary>
public class ApiErrorException : Exception
{
    /// <summary>
    /// The main error message from the API response.
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Additional error details from the API response (optional).
    /// </summary>
    public string? Details { get; }

    /// <summary>
    /// HTTP status code of the error response.
    /// </summary>
    public int StatusCode { get; }

    public ApiErrorException(string error, string? details = null, int statusCode = 0) 
        : base(error)
    {
        Error = error;
        Details = details;
        StatusCode = statusCode;
    }

    public ApiErrorException(string error, string? details, int statusCode, Exception innerException) 
        : base(error, innerException)
    {
        Error = error;
        Details = details;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets a formatted error message including details if available.
    /// </summary>
    public string GetDetailedMessage()
    {
        if (!string.IsNullOrWhiteSpace(Details))
        {
            return $"{Error}\n\nDetails: {Details}";
        }
        return Error;
    }
}
