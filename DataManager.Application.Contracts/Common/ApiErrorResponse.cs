using System.Text.Json.Serialization;

namespace DataManager.Application.Contracts.Common;

/// <summary>
/// Represents the error response structure returned by the API.
/// </summary>
public class ApiErrorResponse
{
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public string? Details { get; set; }
}
