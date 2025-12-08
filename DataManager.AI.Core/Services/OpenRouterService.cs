using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataManager.AI.Core.Services;

/// <summary>
/// Service for interacting with OpenRouter AI API for translations.
/// Uses free models when available.
/// </summary>
public class OpenRouterService : IOpenRouterService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenRouterService> _logger;
    private readonly string _apiKey;
    private readonly string _model;

    public OpenRouterService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenRouterService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["OpenRouter:ApiKey"] ?? string.Empty;
        // Using a free model from OpenRouter - google/gemma-2-9b-it is a good free option
        _model = configuration["OpenRouter:Model"] ?? "google/gemma-2-9b-it:free";
        
        _httpClient.BaseAddress = new Uri("https://openrouter.ai/api/");
        if (!string.IsNullOrEmpty(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }
        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/lsurma/data-manager");
    }

    public async Task<Dictionary<string, string>> TranslateTextAsync(
        string text,
        string sourceCulture,
        List<string> targetCultures,
        string? context = null,
        string? model = null,
        CancellationToken cancellationToken = default)
    {
        // Use provided model or fall back to configured model
        var modelToUse = model ?? _model;
        // Sanitize inputs to prevent prompt injection
        var sanitizedText = SanitizeInput(text);
        var sanitizedSourceCulture = SanitizeInput(sourceCulture);
        var sanitizedTargetCultures = targetCultures.Select(SanitizeInput).ToList();
        var sanitizedContext = context != null ? SanitizeInput(context) : null;

        // Send single request for all target cultures
        var systemPrompt = "You are a professional translator. Translate the given text accurately while preserving the meaning and tone. " +
                          "CRITICAL RULES:\n" +
                          "1. Do NOT translate HTML tags or attributes (e.g., <div>, <span>, class=\"...\", etc.) - keep them exactly as they are\n" +
                          "2. Do NOT translate content inside curly braces {{ }} - these are template variables (e.g., {{ data.name }}, {{ user.email }})\n" +
                          "3. Only translate the actual text content between HTML tags and outside template variables\n" +
                          "4. Preserve ALL formatting, line breaks, newlines, and special characters EXACTLY as they appear in the original text\n" +
                          "5. If the text has multiple lines, your translation MUST also have the same line structure";
        
        if (!string.IsNullOrEmpty(sanitizedContext))
        {
            systemPrompt += $"\nContext: {sanitizedContext}";
        }

        systemPrompt += "\n\nIMPORTANT: You must respond with ONLY the translations in the exact order requested. Do not include any explanations, labels, or additional text.";

        // Build the prompt to request all translations at once
        var targetCulturesText = string.Join(", ", sanitizedTargetCultures);
        const string separator = "---TRANSLATION_SEPARATOR---";
        var userPrompt = $"Translate the following text from {sanitizedSourceCulture} to these languages in order: {targetCulturesText}.\n\n" +
                        $"TEXT TO TRANSLATE:\n{sanitizedText}\n\n" +
                        $"REMEMBER: Keep HTML tags and {{ template.variables }} unchanged! Preserve ALL line breaks and formatting!\n\n" +
                        $"Provide {sanitizedTargetCultures.Count} translations in the exact order: {targetCulturesText}.\n" +
                        $"Separate each translation with this exact control line on its own line:\n{separator}\n\n" +
                        $"IMPORTANT: The separator must be on its own line, with no spaces or other characters.";

        var request = new OpenRouterRequest
        {
            Model = modelToUse,
            Messages = new List<OpenRouterMessage>
            {
                new() { Role = "system", Content = systemPrompt },
                new() { Role = "user", Content = userPrompt }
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("v1/chat/completions", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var openRouterResponse = await response.Content.ReadFromJsonAsync<OpenRouterResponse>(cancellationToken);
            
            if (openRouterResponse?.Choices != null && openRouterResponse.Choices.Count > 0)
            {
                var content = openRouterResponse.Choices[0].Message?.Content?.Trim();
                if (!string.IsNullOrEmpty(content))
                {
                    return ParseTranslations(content, sanitizedTargetCultures);
                }
            }

            _logger.LogWarning("OpenRouter returned empty response for translation from {SourceCulture} to {TargetCultures}", sanitizedSourceCulture, string.Join(", ", sanitizedTargetCultures));
            
            // Return original text for all cultures as fallback
            return sanitizedTargetCultures.ToDictionary(c => c, c => text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Translation error from {SourceCulture} to {TargetCultures}: {ErrorMessage}", sanitizedSourceCulture, string.Join(", ", sanitizedTargetCultures), ex.Message);
            
            // Return original text for all cultures as fallback
            return sanitizedTargetCultures.ToDictionary(c => c, c => text);
        }
    }

    /// <summary>
    /// Sanitizes input to prevent prompt injection attacks.
    /// </summary>
    private string SanitizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Remove potential prompt injection patterns
        var sanitized = input
            // Remove system-level directives
            .Replace("system:", "", StringComparison.OrdinalIgnoreCase)
            .Replace("assistant:", "", StringComparison.OrdinalIgnoreCase)
            .Replace("user:", "", StringComparison.OrdinalIgnoreCase)
            // Remove common injection patterns
            .Replace("ignore previous instructions", "", StringComparison.OrdinalIgnoreCase)
            .Replace("ignore above", "", StringComparison.OrdinalIgnoreCase)
            .Replace("disregard", "", StringComparison.OrdinalIgnoreCase);

        // Limit length to prevent extremely long inputs
        const int maxLength = 5000;
        if (sanitized.Length > maxLength)
        {
            sanitized = sanitized.Substring(0, maxLength);
            _logger.LogWarning("Input truncated from {OriginalLength} to {MaxLength} characters", input.Length, maxLength);
        }

        return sanitized;
    }

    /// <summary>
    /// Parses the AI response containing multiple translations separated by delimiters.
    /// Preserves multi-line text and all formatting exactly as returned by the AI.
    /// </summary>
    private Dictionary<string, string> ParseTranslations(string content, List<string> targetCultures)
    {
        var result = new Dictionary<string, string>();
        const string separator = "---TRANSLATION_SEPARATOR---";

        // Split by the primary separator
        var translations = content.Split(new[] { separator }, StringSplitOptions.None)
            .Select(t => t.Trim('\r', '\n'))  // Only trim carriage returns and newlines at start/end, preserve internal formatting
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();

        // If no primary separator found, try fallback separators
        if (translations.Count == 1 && targetCultures.Count > 1)
        {
            // Try with shorter separator (legacy)
            translations = content.Split(new[] { "---" }, StringSplitOptions.None)
                .Select(t => t.Trim('\r', '\n'))
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
        }

        // If still only one item and we expect multiple, try triple newlines as final fallback
        // Triple newlines are used as they're unlikely to appear naturally in formatted text
        // while still providing a reasonable delimiter when AI doesn't follow separator instructions
        if (translations.Count == 1 && targetCultures.Count > 1)
        {
            translations = content.Split(new[] { "\n\n\n" }, StringSplitOptions.None)
                .Select(t => t.Trim('\r', '\n'))
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
        }

        // Map translations to cultures
        for (int i = 0; i < targetCultures.Count; i++)
        {
            if (i < translations.Count)
            {
                result[targetCultures[i]] = translations[i];
            }
            else
            {
                // If we don't have enough translations, log warning and use the first one or empty
                _logger.LogWarning("Not enough translations returned for culture {Culture}. Expected {Expected}, got {Actual}", 
                    targetCultures[i], targetCultures.Count, translations.Count);
                result[targetCultures[i]] = translations.Count > 0 ? translations[0] : string.Empty;
            }
        }

        return result;
    }
}

// DTOs for OpenRouter API
internal class OpenRouterRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<OpenRouterMessage> Messages { get; set; } = new();
}

internal class OpenRouterMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

internal class OpenRouterResponse
{
    [JsonPropertyName("choices")]
    public List<OpenRouterChoice> Choices { get; set; } = new();
}

internal class OpenRouterChoice
{
    [JsonPropertyName("message")]
    public OpenRouterMessage? Message { get; set; }
}
