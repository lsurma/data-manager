namespace DataManager.Application.Core.Common;

/// <summary>
/// Service that provides a predefined list of available cultures in the system
/// </summary>
public class CultureService : ICultureService
{
    private static readonly List<string> _availableCultures = new()
    {
        "en-US",
        "en-GB",
        "pl-PL",
        "de-DE",
        "fr-FR",
        "es-ES",
        "it-IT",
        "nl-NL",
        "pt-PT",
        "cs-CZ",
        "sk-SK",
        "hu-HU",
        "ro-RO",
        "bg-BG"
    };

    /// <summary>
    /// Gets all available culture codes in the system
    /// </summary>
    public List<string> GetAvailableCultures()
    {
        return _availableCultures.OrderBy(c => c).ToList();
    }

    /// <summary>
    /// Checks if a culture code is valid in the system
    /// </summary>
    public bool IsValidCulture(string? cultureCode)
    {
        if (string.IsNullOrWhiteSpace(cultureCode))
            return false;

        return _availableCultures.Contains(cultureCode, StringComparer.OrdinalIgnoreCase);
    }
}
