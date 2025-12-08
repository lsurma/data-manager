namespace DataManager.Application.Contracts.Modules.Translations;

/// <summary>
/// Result of the index translations operation
/// </summary>
public record IndexTranslationsResult
{
    /// <summary>
    /// Number of translations updated
    /// </summary>
    public int UpdatedCount { get; init; }

    /// <summary>
    /// Number of translations processed
    /// </summary>
    public int ProcessedCount { get; init; }

    /// <summary>
    /// List of errors encountered during the operation
    /// </summary>
    public List<string> Errors { get; init; } = new List<string>();
}
