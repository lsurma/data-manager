namespace DataManager.Application.Contracts.Common;

/// <summary>
/// Represents an optional value that can be either set or not set.
/// This is useful for distinguishing between a value that is explicitly null vs a value that wasn't provided at all.
/// </summary>
/// <typeparam name="T">The type of the optional value</typeparam>
public struct Optional<T>
{
    private readonly T? _value;
    private readonly bool _isSet;

    /// <summary>
    /// Creates an Optional with a value.
    /// </summary>
    public Optional(T? value)
    {
        _value = value;
        _isSet = true;
    }

    /// <summary>
    /// Indicates whether the value has been set (even if set to null).
    /// </summary>
    public bool IsSet => _isSet;

    /// <summary>
    /// Gets the value if set, otherwise throws InvalidOperationException.
    /// </summary>
    public T? Value
    {
        get
        {
            if (!_isSet)
                throw new InvalidOperationException("Optional value has not been set.");
            return _value;
        }
    }

    /// <summary>
    /// Gets the value if set, otherwise returns the default value for the type.
    /// </summary>
    public T? GetValueOrDefault() => _isSet ? _value : default;

    /// <summary>
    /// Gets the value if set, otherwise returns the provided default value.
    /// </summary>
    public T GetValueOrDefault(T defaultValue) => _isSet ? _value! : defaultValue;

    /// <summary>
    /// Creates an Optional with no value set.
    /// </summary>
    public static Optional<T> Unset() => default;

    /// <summary>
    /// Implicit conversion from T to Optional&lt;T&gt;.
    /// </summary>
    public static implicit operator Optional<T>(T? value) => new(value);

    /// <summary>
    /// Explicit conversion from Optional&lt;T&gt; to T.
    /// </summary>
    public static explicit operator T?(Optional<T> optional) => optional.Value;

    public override string ToString()
    {
        return _isSet ? $"Optional({_value})" : "Optional(Unset)";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Optional<T> other)
            return false;

        if (!_isSet && !other._isSet)
            return true;

        if (_isSet != other._isSet)
            return false;

        return EqualityComparer<T>.Default.Equals(_value, other._value);
    }

    public override int GetHashCode()
    {
        if (!_isSet)
            return 0;

        return _value?.GetHashCode() ?? 0;
    }

    public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);
    public static bool operator !=(Optional<T> left, Optional<T> right) => !left.Equals(right);
}
