namespace DataManager.Application.Contracts.Modules.DataSets;

public record DataSetDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// List of user/application identity IDs that have access to this DataSet.
    /// If empty, the DataSet has public access (no restrictions).
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
    public ICollection<string> WebhookUrls { get; set; } = new List<string>();

    public ICollection<Guid> IncludedDataSetIds { get; set; } = new List<Guid>();

    public ICollection<DataSetDto> IncludedDataSets { get; set; } = new List<DataSetDto>();

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}
