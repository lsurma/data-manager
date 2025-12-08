namespace DataManager.Application.Contracts.Modules.Translations;

/// <summary>
/// Result of the remove duplicate translations operation
/// </summary>
public record RemoveDuplicateTranslationsResult
{
    /// <summary>
    /// Number of duplicate translations removed
    /// </summary>
    public int RemovedCount { get; init; }

    /// <summary>
    /// Number of translations processed from the specific dataset
    /// </summary>
    public int ProcessedCount { get; init; }

    /// <summary>
    /// List of errors encountered during the operation
    /// </summary>
    public List<string> Errors { get; init; } = new List<string>();
}
