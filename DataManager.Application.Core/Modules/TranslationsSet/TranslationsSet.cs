using DataManager.Application.Core.Abstractions;

namespace DataManager.Application.Core.Modules.TranslationsSet;

public class TranslationsSet : AuditableEntityBase
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// List of user/application identity IDs that have access to this TranslationsSet.
    /// If empty or null, no access restrictions are applied (public access).
    /// </summary>
    public ICollection<string> AllowedIdentityIds { get; set; } = new List<string>();

    /// <summary>
    /// List of culture codes available for this TranslationsSet (e.g., "en-US", "pl-PL").
    /// If empty or null, all system cultures are available.
    /// </summary>
    public ICollection<string>? AvailableCultures { get; set; }

    public ICollection<TranslationsSetInclude> Includes { get; set; } = new List<TranslationsSetInclude>();

    public ICollection<TranslationsSetInclude> IncludedIn { get; set; } = new List<TranslationsSetInclude>();
}
