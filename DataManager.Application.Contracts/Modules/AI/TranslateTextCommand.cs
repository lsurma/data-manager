using MediatR;

namespace DataManager.Application.Contracts.Modules.AI;

/// <summary>
/// Command to translate text from one culture to one or more target cultures using AI.
/// </summary>
public class TranslateTextCommand : IRequest<TranslateTextResult>
{
    /// <summary>
    /// The text to translate.
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// Source culture code (e.g., "en-US", "pl-PL").
    /// </summary>
    public required string SourceCulture { get; set; }

    /// <summary>
    /// List of target culture codes to translate to.
    /// </summary>
    public required List<string> TargetCultures { get; set; }

    /// <summary>
    /// Optional context to provide to the AI (e.g., "ecommerce", "technical", "casual").
    /// This is not required but helps improve translation quality.
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Optional model to use for translation.
    /// If not specified, uses the default model from configuration.
    /// </summary>
    public string? Model { get; set; }
}
