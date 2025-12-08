using DataManager.Application.Core.Abstractions;

namespace DataManager.Application.Core.Modules.Log;

public class Log : AuditableEntityBase
{
    /// <summary>
    /// Type of log entry (e.g., "Webhook", "Email", etc.)
    /// </summary>
    public required string LogType { get; set; }

    /// <summary>
    /// Action being performed (e.g., "Send", "Notification")
    /// </summary>
    public required string Action { get; set; }

    /// <summary>
    /// Target URL for webhooks, email address for emails, etc.
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// Status of the operation (e.g., "Started", "Success", "Failed")
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// When the operation started
    /// </summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>
    /// When the operation ended (null if still in progress)
    /// </summary>
    public DateTimeOffset? EndedAt { get; set; }

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional details about the operation
    /// </summary>
    public string? Details { get; set; }
}
