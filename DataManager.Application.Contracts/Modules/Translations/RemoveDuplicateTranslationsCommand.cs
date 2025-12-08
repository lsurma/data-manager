using MediatR;

namespace DataManager.Application.Contracts.Modules.Translations;

/// <summary>
/// Command to remove duplicate translations from a specific dataset that also exist in a base dataset.
/// Translations are considered duplicates when TranslationKey, CultureName, and Content match.
/// </summary>
public class RemoveDuplicateTranslationsCommand : IRequest<RemoveDuplicateTranslationsResult>
{
    /// <summary>
    /// The dataset ID from which duplicates will be removed
    /// </summary>
    public required Guid SpecificDataSetId { get; set; }

    /// <summary>
    /// The base dataset ID containing the canonical translations
    /// </summary>
    public required Guid BaseDataSetId { get; set; }
}
