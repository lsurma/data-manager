using DataManager.Application.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Common.BackgroundJobs;

/// <summary>
/// Helper service for notifying webhooks when DataSet data changes.
/// </summary>
public class WebhookNotificationHelper
{
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly DataManagerDbContext _context;

    public WebhookNotificationHelper(
        IBackgroundJobService backgroundJobService,
        DataManagerDbContext context)
    {
        _backgroundJobService = backgroundJobService;
        _context = context;
    }

    /// <summary>
    /// Notifies all webhooks registered for a DataSet that data has changed.
    /// </summary>
    /// <param name="translationSetId">The ID of the DataSet that changed</param>
    /// <param name="eventType">The type of event (e.g., "translation.updated", "translationset.changed")</param>
    /// <param name="additionalData">Optional additional data to include in the webhook payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Job ID if webhooks were scheduled, null if no webhooks configured</returns>
    public async Task<string?> NotifyTranslationSetChangeAsync(
        Guid translationSetId,
        string eventType,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default)
    {
        // Fetch the DataSet to get webhook URLs and secret key
        var dataSet = await _context.DataSets
            .AsNoTracking()
            .Where(ts => ts.Id == translationSetId)
            .Select(ts => new
            {
                ts.WebhookUrls,
                ts.SecretKey
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dataSet == null)
        {
            throw new InvalidOperationException($"DataSet with ID {translationSetId} not found");
        }

        // Check if there are any webhook URLs configured
        if (dataSet.WebhookUrls == null || !dataSet.WebhookUrls.Any())
        {
            // No webhooks configured, nothing to do
            return null;
        }

        // Create the webhook notification request
        var request = new WebhookNotificationRequest
        {
            WebhookUrls = dataSet.WebhookUrls.ToList(),
            SecretKey = dataSet.SecretKey,
            Payload = new WebhookPayload
            {
                EventType = eventType,
                DataSetId = translationSetId,
                Data = additionalData
            }
        };

        // Schedule the webhook notification
        var jobId = await _backgroundJobService.ScheduleWebhookNotificationAsync(request, cancellationToken);
        
        return jobId;
    }

    /// <summary>
    /// Notifies webhooks for multiple DataSets that share data changes.
    /// Useful when a change affects multiple datasets.
    /// </summary>
    /// <param name="translationSetIds">The IDs of DataSets that changed</param>
    /// <param name="eventType">The type of event</param>
    /// <param name="additionalData">Optional additional data to include in the webhook payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary mapping DataSet ID to Job ID (or null if no webhooks)</returns>
    public async Task<Dictionary<Guid, string?>> NotifyMultipleTranslationSetsChangeAsync(
        IEnumerable<Guid> translationSetIds,
        string eventType,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<Guid, string?>();
        
        foreach (var translationSetId in translationSetIds)
        {
            try
            {
                var jobId = await NotifyTranslationSetChangeAsync(
                    translationSetId,
                    eventType,
                    additionalData,
                    cancellationToken);
                
                results[translationSetId] = jobId;
            }
            catch (Exception)
            {
                // Continue with other sets even if one fails
                // Store null to indicate failure
                results[translationSetId] = null;
            }
        }
        
        return results;
    }
}
