namespace DataManager.Application.Contracts.Modules.Log;

public record LogDto
{
    public Guid Id { get; set; }

    public string LogType { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? Target { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? EndedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public string? Details { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}
