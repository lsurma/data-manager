using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataManager.Application.Contracts.Common;

/// <summary>
/// JSON converter for Optional&lt;T&gt; type.
/// Handles serialization and deserialization of Optional values.
/// </summary>
public class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // If the token is null, we still want to mark it as specified (explicitly null)
        if (reader.TokenType == JsonTokenType.Null)
        {
            return Optional<T>.Null();
        }

        // Deserialize the value
        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return Optional<T>.Of(value);
    }

    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        if (!value.IsSpecified)
        {
            // Write null for unspecified values to maintain JSON structure consistency.
            // This allows the property to appear in the JSON but distinguishes unspecified from explicitly set.
            // When deserializing, if the property is present (even as null), it will be marked as specified.
            writer.WriteNullValue();
        }
        else
        {
            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }
}

/// <summary>
/// Factory for creating OptionalJsonConverter instances.
/// </summary>
public class OptionalJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        return typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type valueType = typeToConvert.GetGenericArguments()[0];

        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            typeof(OptionalJsonConverter<>).MakeGenericType(valueType))!;

        return converter;
    }
}
