using MediatR;

namespace DataManager.Application.Contracts.Modules.Translations;

/// <summary>
/// Command to index translations by setting InternalGroupName1 and InternalGroupName2 based on TranslationName patterns.
/// - Sets InternalGroupName1 to "Email" when TranslationName starts with "Email."
/// - Sets InternalGroupName2 to "EmailLayout" when TranslationName contains "Layout"
/// </summary>
public class IndexTranslationsCommand : IRequest<IndexTranslationsResult>
{
    /// <summary>
    /// Optional dataset ID to filter translations. If null, processes all translations.
    /// </summary>
    public Guid? DataSetId { get; set; }
}
