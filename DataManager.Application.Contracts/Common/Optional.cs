using System.Text.Json.Serialization;

namespace DataManager.Application.Contracts.Common;

/// <summary>
/// Represents an optional value that distinguishes between:
/// - Not specified (IsSpecified = false)
/// - Explicitly set to null (IsSpecified = true, Value = null)
/// - Set to a value (IsSpecified = true, Value = non-null)
/// 
/// Useful for PATCH-style operations where you only want to update fields that were explicitly provided.
/// </summary>
/// <typeparam name="T">The type of the value</typeparam>
[JsonConverter(typeof(OptionalJsonConverterFactory))]
public struct Optional<T>
{
    private readonly T _value;
    private readonly bool _isSpecified;

    /// <summary>
    /// Gets whether this optional value was explicitly specified.
    /// </summary>
    [JsonIgnore]
    public bool IsSpecified => _isSpecified;

    /// <summary>
    /// Gets the value if specified. Throws if not specified.
    /// </summary>
    [JsonIgnore]
    public T Value
    {
        get
        {
            if (!_isSpecified)
            {
                throw new InvalidOperationException("Cannot access value of unspecified Optional<T>.");
            }
            return _value;
        }
    }

    /// <summary>
    /// Creates an Optional with a specified value.
    /// </summary>
    public Optional(T value)
    {
        _value = value;
        _isSpecified = true;
    }

    /// <summary>
    /// Creates an unspecified Optional.
    /// </summary>
    public static Optional<T> Unspecified() => default;

    /// <summary>
    /// Creates an Optional with a null value.
    /// </summary>
    public static Optional<T> Null() => new Optional<T>(default(T)!);

    /// <summary>
    /// Creates an Optional with a specified value.
    /// </summary>
    public static Optional<T> Of(T value) => new Optional<T>(value);

    /// <summary>
    /// Implicitly converts a value to an Optional.
    /// </summary>
    public static implicit operator Optional<T>(T value) => new Optional<T>(value);

    /// <summary>
    /// Gets the value or a default value if not specified.
    /// </summary>
    public T GetValueOrDefault(T defaultValue) => _isSpecified ? _value : defaultValue;

    public override string ToString()
    {
        return _isSpecified ? $"Optional({_value})" : "Optional(unspecified)";
    }
}
