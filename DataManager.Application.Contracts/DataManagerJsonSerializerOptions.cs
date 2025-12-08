using System.Text.Json;
using System.Text.Json.Serialization;
using DataManager.Application.Contracts.Common;

namespace DataManager.Application.Contracts;

/// <summary>
/// Provides centralized JSON serializer configuration for DataManager applications.
/// Ensures consistent serialization behavior across API and client applications.
/// </summary>
public static class DataManagerJsonSerializerOptions
{
    /// <summary>
    /// Gets the default JSON serializer options configured with all necessary converters.
    /// Includes support for Optional&lt;T&gt; types, case-insensitive property matching, and reference cycle handling.
    /// </summary>
    public static JsonSerializerOptions Default { get; } = CreateDefault();

    /// <summary>
    /// Creates a new instance of JsonSerializerOptions with default configuration.
    /// Use this when you need to customize options while maintaining the base configuration.
    /// </summary>
    public static JsonSerializerOptions CreateDefault()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        // Add custom converters
        options.Converters.Add(new OptionalJsonConverterFactory());
        options.Converters.Add(new QueryFilterJsonConverter());

        return options;
    }
}
