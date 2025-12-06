using DataManager.Application.Core.Abstractions;

namespace DataManager.Application.Core.Modules.TranslationSet;

public class TranslationSet : AuditableEntityBase
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// List of user/application identity IDs that have access to this TranslationSet.
    /// If empty or null, no access restrictions are applied (public access).
    /// </summary>
    public ICollection<string> AllowedIdentityIds { get; set; } = new List<string>();

    /// <summary>
    /// List of culture codes available for this TranslationSet (e.g., "en-US", "pl-PL").
    /// If empty or null, all system cultures are available.
    /// </summary>
    public ICollection<string>? AvailableCultures { get; set; }

    public ICollection<TranslationSetInclude> Includes { get; set; } = new List<TranslationSetInclude>();

    public ICollection<TranslationSetInclude> IncludedIn { get; set; } = new List<TranslationSetInclude>();
}
