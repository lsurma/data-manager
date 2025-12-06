using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataManager.Application.Contracts.Common;

/// <summary>
/// JSON converter factory for Optional&lt;T&gt; types.
/// Handles serialization/deserialization of optional values, distinguishing between unset and null.
/// 
/// Important: Properties using Optional&lt;T&gt; should have the JsonConverter attribute:
/// [JsonConverter(typeof(OptionalJsonConverterFactory))]
/// public Optional&lt;string&gt; Name { get; set; }
/// </summary>
public class OptionalJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        return typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type valueType = typeToConvert.GetGenericArguments()[0];

        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            typeof(OptionalJsonConverter<>).MakeGenericType(valueType),
            args: null)!;

        return converter;
    }
}

/// <summary>
/// JSON converter for Optional&lt;T&gt; type.
/// This converter properly handles the distinction between:
/// - Property present with value: {"Name": "test"} -> Optional with "test"
/// - Property present with null: {"Name": null} -> Optional with null
/// - Property absent: {} -> Optional unset
/// </summary>
internal class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // If we're here, the property exists in the JSON (either with a value or null)
        // So we create an Optional with a value (which could be null)
        if (reader.TokenType == JsonTokenType.Null)
        {
            // Property is present with null value: {"Name": null}
            return new Optional<T>(default);
        }

        T? value = JsonSerializer.Deserialize<T>(ref reader, options);
        return new Optional<T>(value);
    }

    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        if (!value.IsSet)
        {
            // When serializing an unset Optional, write null
            // The calling code can use JsonIgnoreCondition.WhenWritingDefault to omit it
            writer.WriteNullValue();
        }
        else
        {
            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }
}
