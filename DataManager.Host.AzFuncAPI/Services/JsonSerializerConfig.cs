using System.Text.Json;
using System.Text.Json.Serialization;
using DataManager.Application.Contracts.Common;

namespace DataManager.Host.AzFuncAPI.Services;

/// <summary>
/// Provides centrally configured JsonSerializerOptions for the application.
/// This ensures consistent JSON serialization/deserialization across all controllers.
/// </summary>
public static class JsonSerializerConfig
{
    /// <summary>
    /// Gets the default JsonSerializerOptions configured for requests (deserialization).
    /// Includes OptionalJsonConverterFactory for automatic Optional&lt;T&gt; handling.
    /// </summary>
    public static JsonSerializerOptions Default { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new OptionalJsonConverterFactory()
        }
    };

    /// <summary>
    /// Gets the JsonSerializerOptions configured for responses (serialization).
    /// Includes OptionalJsonConverterFactory and handles reference cycles.
    /// </summary>
    public static JsonSerializerOptions Response { get; } = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Converters =
        {
            new OptionalJsonConverterFactory()
        }
    };

    /// <summary>
    /// Gets the JsonSerializerOptions configured for filtering (with QueryFilterJsonConverter).
    /// </summary>
    public static JsonSerializerOptions Filtering { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new QueryFilterJsonConverter(),
            new OptionalJsonConverterFactory()
        }
    };
}
