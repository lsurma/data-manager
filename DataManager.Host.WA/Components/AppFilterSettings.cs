namespace DataManager.Host.WA.Components;

/// <summary>
/// Application-specific settings for filters that are managed separately from grid settings.
/// This allows us to track and persist filter visibility and order preferences.
/// </summary>
public record AppFilterSettings
{
    /// <summary>
    /// Filter visibility and order settings
    /// </summary>
    public List<FilterSettings> Filters { get; set; } = new();
}
