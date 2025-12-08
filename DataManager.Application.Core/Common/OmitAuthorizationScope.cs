namespace DataManager.Application.Core.Common;

/// <summary>
/// Provides a way to temporarily omit authorization checks within a scope.
/// Useful when authorization has already been performed in an outer query/command
/// and you want to avoid redundant authorization checks for better performance.
/// 
/// Usage:
/// <code>
/// using (new OmitAuthorizationScope())
/// {
///     // Authorization will be omitted within this scope
///     var result = await _queryService.PrepareQueryAsync(...);
/// }
/// </code>
/// </summary>
public sealed class OmitAuthorizationScope : IDisposable
{
    private static readonly AsyncLocal<bool> _omitAuthorization = new();
    
    private readonly bool _previousValue;

    /// <summary>
    /// Creates a new scope where authorization is omitted.
    /// </summary>
    public OmitAuthorizationScope()
    {
        _previousValue = _omitAuthorization.Value;
        _omitAuthorization.Value = true;
    }

    /// <summary>
    /// Gets a value indicating whether authorization should be omitted in the current async context.
    /// </summary>
    public static bool ShouldOmitAuthorization => _omitAuthorization.Value;

    /// <summary>
    /// Disposes the scope and restores the previous authorization state.
    /// </summary>
    public void Dispose()
    {
        _omitAuthorization.Value = _previousValue;
    }
}
