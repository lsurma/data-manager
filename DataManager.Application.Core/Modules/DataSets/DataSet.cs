using DataManager.Application.Core.Abstractions;

namespace DataManager.Application.Core.Modules.DataSets;

public class DataSet : AuditableEntityBase
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// List of user/application identity IDs that have access to this DataSet.
    /// If empty or null, no access restrictions are applied (public access).
    /// </summary>
    public ICollection<string> AllowedIdentityIds { get; set; } = new List<string>();

    /// <summary>
    /// List of culture codes available for this DataSet (e.g., "en-US", "pl-PL").
    /// If empty, all system cultures are available.
    /// </summary>
    public ICollection<string> AvailableCultures { get; set; } = new List<string>();

    /// <summary>
    /// Secret key that will be sent in webhook requests for authentication/verification.
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// List of webhook URLs to which events will be sent.
    /// </summary>
    public ICollection<Uri> WebhookUrls { get; set; } = new List<Uri>();

    public ICollection<DataSetInclude> Includes { get; set; } = new List<DataSetInclude>();

    public ICollection<DataSetInclude> IncludedIn { get; set; } = new List<DataSetInclude>();
}
