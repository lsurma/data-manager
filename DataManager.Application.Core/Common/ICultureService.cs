namespace DataManager.Application.Core.Common;

public interface ICultureService
{
    /// <summary>
    /// Gets all available culture codes in the system
    /// </summary>
    List<string> GetAvailableCultures();

    /// <summary>
    /// Checks if a culture code is valid in the system
    /// </summary>
    bool IsValidCulture(string? cultureCode);
}
