namespace DataManager.Host.WA.Models;

/// <summary>
/// Application settings stored in local storage
/// </summary>
public record AppSettings
{
    public ThemeSettings Theme { get; init; } = new();
}

/// <summary>
/// Theme customization settings
/// </summary>
public record ThemeSettings
{
    /// <summary>
    /// Primary accent color in hex format (e.g., #1060ff)
    /// </summary>
    public string PrimaryColor { get; init; } = "#1060ff";
    
    /// <summary>
    /// Selected predefined theme name
    /// </summary>
    public string SelectedTheme { get; init; } = "Default";
}

/// <summary>
/// Predefined theme options
/// </summary>
public static class PredefinedThemes
{
    public static readonly Dictionary<string, ThemeSettings> Themes = new()
    {
        ["Default"] = new ThemeSettings
        {
            PrimaryColor = "#1060ff",
            SelectedTheme = "Default"
        },
        ["Purple"] = new ThemeSettings
        {
            PrimaryColor = "#7c3aed",
            SelectedTheme = "Purple"
        },
        ["Green"] = new ThemeSettings
        {
            PrimaryColor = "#059669",
            SelectedTheme = "Green"
        },
        ["Orange"] = new ThemeSettings
        {
            PrimaryColor = "#ea580c",
            SelectedTheme = "Orange"
        },
        ["Pink"] = new ThemeSettings
        {
            PrimaryColor = "#db2777",
            SelectedTheme = "Pink"
        },
        ["Custom"] = new ThemeSettings
        {
            PrimaryColor = "#1060ff",
            SelectedTheme = "Custom"
        }
    };
    
    public static ThemeSettings GetDefaultTheme() => Themes["Default"];
    
    public static List<string> GetThemeNames() => Themes.Keys.ToList();
}
