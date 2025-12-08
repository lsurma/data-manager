namespace DataManager.Application.Core.Common.BackgroundJobs;

/// <summary>
/// Service for scheduling and executing background jobs.
/// Implementation can be in-memory for local development or Azure Durable Functions for production.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Schedules a webhook notification job to be executed in the background.
    /// </summary>
    /// <param name="request">The webhook notification request containing webhook URLs and payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Job ID for tracking purposes</returns>
    Task<string> ScheduleWebhookNotificationAsync(
        WebhookNotificationRequest request,
        CancellationToken cancellationToken = default);
}
