using System.Text.Json.Serialization;

namespace DataManager.Application.Contracts.Common;

/// <summary>
/// Represents the error response structure returned by the API.
/// </summary>
public class ApiErrorResponse
{
    [JsonPropertyName("error")]
    public required string Error { get; set; }

    [JsonPropertyName("details")]
    public string? Details { get; set; }
}
