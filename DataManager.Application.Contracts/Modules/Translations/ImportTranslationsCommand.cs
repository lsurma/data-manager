using MediatR;

namespace DataManager.Application.Contracts.Modules.Translations;

/// <summary>
/// Command to import multiple translations from a remote source
/// </summary>
public class ImportTranslationsCommand : IRequest<ImportTranslationsResult>
{
    public required Guid DataSetId { get; set; }
    public required List<ImportTranslationInput> Translations { get; set; }
}

/// <summary>
/// Simplified translation data for importing from remote sources
/// </summary>
public record ImportTranslationInput
{
    public string? CultureName { get; set; }
    public required string ResourceName { get; set; }
    public required string TranslationName { get; set; }
    public required string Content { get; set; }
    
    public DateTimeOffset? ContentUpdatedAt { get; set; }
}

/// <summary>
/// Result of import operation
/// </summary>
public record ImportTranslationsResult
{
    public int ImportedCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
