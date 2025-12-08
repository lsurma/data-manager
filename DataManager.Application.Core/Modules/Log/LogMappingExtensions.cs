using DataManager.Application.Contracts.Modules.Log;

namespace DataManager.Application.Core.Modules.Log;

public static class LogMappingExtensions
{
    public static LogDto ToDto(this Log log)
    {
        return new LogDto
        {
            Id = log.Id,
            LogType = log.LogType,
            Action = log.Action,
            Target = log.Target,
            Status = log.Status,
            StartedAt = log.StartedAt,
            EndedAt = log.EndedAt,
            ErrorMessage = log.ErrorMessage,
            Details = log.Details,
            CreatedAt = log.CreatedAt,
            UpdatedAt = log.UpdatedAt,
            CreatedBy = log.CreatedBy
        };
    }

    public static List<LogDto> ToDto(this List<Log> logs)
    {
        return logs.Select(ToDto).ToList();
    }
}
