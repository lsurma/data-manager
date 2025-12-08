namespace DataManager.Application.Core.Common.BackgroundJobs;

/// <summary>
/// Request for sending webhook notifications to registered endpoints.
/// </summary>
public record WebhookNotificationRequest
{
    /// <summary>
    /// List of webhook URLs to notify.
    /// </summary>
    public required IReadOnlyList<Uri> WebhookUrls { get; init; }

    /// <summary>
    /// Secret key to include in the webhook request for authentication/verification.
    /// </summary>
    public string? SecretKey { get; init; }

    /// <summary>
    /// The payload to send in the webhook notification.
    /// </summary>
    public required WebhookPayload Payload { get; init; }
}

/// <summary>
/// Base payload for webhook notifications.
/// </summary>
public record WebhookPayload
{
    /// <summary>
    /// Type of event that triggered the webhook (e.g., "translation.updated", "translationset.changed").
    /// </summary>
    public required string EventType { get; init; }

    /// <summary>
    /// Timestamp when the event occurred (UTC).
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// ID of the DataSet that was affected.
    /// </summary>
    public required Guid DataSetId { get; init; }

    /// <summary>
    /// Additional data specific to the event type.
    /// </summary>
    public Dictionary<string, object>? Data { get; init; }
}
