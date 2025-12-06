using System.Text.Json;
using System.Text.Json.Serialization;
using DataManager.Application.Contracts.Common;

namespace DataManager.Host.WA.Services;

/// <summary>
/// Provides centrally configured JsonSerializerOptions for the WebAssembly application.
/// This ensures consistent JSON serialization/deserialization across the frontend.
/// </summary>
public static class JsonSerializerConfig
{
    /// <summary>
    /// Gets the default JsonSerializerOptions configured for the application.
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
}
