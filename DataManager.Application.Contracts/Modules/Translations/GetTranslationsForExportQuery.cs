using DataManager.Application.Contracts.Common;
using MediatR;

namespace DataManager.Application.Contracts.Modules.Translations;

/// <summary>
/// Simplified query for exporting translations with minimal parameters
/// Returns paginated list of simple translation DTOs
/// </summary>
public class GetTranslationsForExportQuery : IRequest<PaginatedList<SimpleTranslationDto>>
{
    /// <summary>
    /// Culture names to filter translations (e.g., "en-US", "pl-PL")
    /// If not provided or empty, all cultures will be included
    /// </summary>
    public IEnumerable<string> Cultures { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Maximum number of translations to return
    /// </summary>
    public int Limit { get; set; } = 20;

    /// <summary>
    /// Number of translations to skip
    /// </summary>
    public int Offset { get; set; } = 0;

    public DateTimeOffset? ContentUpdatedAtAfter { get; set; }
    
    /// <summary>
    /// Translation set ID to fetch translations from
    /// When SpecificDataSetId is provided, this is used as the hierarchy root
    /// </summary>
    public Guid DataSetId { get; set; }

    /// <summary>
    /// Optional specific translation set ID
    /// When provided, fetches only translations from this specific set without hierarchy traversal
    /// </summary>
    public Guid? SpecificDataSetId { get; set; }
}
