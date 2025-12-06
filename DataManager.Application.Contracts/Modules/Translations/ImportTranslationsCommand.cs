using MediatR;

namespace DataManager.Application.Contracts.Modules.Translations;

/// <summary>
/// Command to import multiple translations from a remote source
/// </summary>
public class ImportTranslationsCommand : IRequest<ImportTranslationsResult>
{
    public required Guid TranslationsSetId { get; set; }
    public required List<ImportTranslationDto> Translations { get; set; }
}

/// <summary>
/// Simplified translation data for importing from remote sources
/// </summary>
public record ImportTranslationDto
{
    public string? CultureName { get; set; }
    public required string ResourceName { get; set; }
    public required string TranslationName { get; set; }
    public required string Content { get; set; }
    public string? InternalGroupName1 { get; set; }
    public string? InternalGroupName2 { get; set; }
    public string? ContentTemplate { get; set; }
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
