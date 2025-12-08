namespace DataManager.AI.Core.Services;

/// <summary>
/// Interface for AI translation service.
/// </summary>
public interface IOpenRouterService
{
    /// <summary>
    /// Translates text from source culture to multiple target cultures.
    /// </summary>
    /// <param name="text">Text to translate</param>
    /// <param name="sourceCulture">Source culture code</param>
    /// <param name="targetCultures">List of target culture codes</param>
    /// <param name="context">Optional context for translation</param>
    /// <param name="model">Optional model to use for translation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of culture codes to translated text</returns>
    Task<Dictionary<string, string>> TranslateTextAsync(
        string text, 
        string sourceCulture, 
        List<string> targetCultures, 
        string? context = null,
        string? model = null,
        CancellationToken cancellationToken = default);
}
