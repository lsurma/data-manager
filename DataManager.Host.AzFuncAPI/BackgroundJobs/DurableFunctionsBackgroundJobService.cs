using DataManager.Application.Core.Common.BackgroundJobs;
using DataManager.Application.Core.Data;
using DataManager.Application.Core.Modules.Log;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DataManager.Host.AzFuncAPI.BackgroundJobs;

/// <summary>
/// Azure Durable Functions implementation of background job service.
/// Uses Durable Task Framework for reliable, scalable background job execution.
/// </summary>
public class DurableFunctionsBackgroundJobService : IBackgroundJobService
{
    private readonly DurableTaskClient _durableTaskClient;
    private readonly ILogger<DurableFunctionsBackgroundJobService> _logger;

    public DurableFunctionsBackgroundJobService(
        DurableTaskClient durableTaskClient,
        ILogger<DurableFunctionsBackgroundJobService> logger)
    {
        _durableTaskClient = durableTaskClient;
        _logger = logger;
    }

    public async Task<string> ScheduleWebhookNotificationAsync(
        WebhookNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var instanceId = await _durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
            nameof(WebhookNotificationOrchestrator),
            request);

        _logger.LogInformation(
            "Scheduled webhook notification orchestration {InstanceId} for {Count} webhook(s)",
            instanceId,
            request.WebhookUrls.Count);

        return instanceId;
    }

    /// <summary>
    /// Orchestrator function for webhook notifications.
    /// Coordinates the sending of webhooks with retry logic.
    /// </summary>
    [Function(nameof(WebhookNotificationOrchestrator))]
    public static async Task WebhookNotificationOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var request = context.GetInput<WebhookNotificationRequest>();
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Webhook notification request cannot be null");
        }

        var logger = context.CreateReplaySafeLogger<DurableFunctionsBackgroundJobService>();
        logger.LogInformation(
            "Starting webhook notification orchestration for {Count} webhook(s)",
            request.WebhookUrls.Count);

        // Send webhooks in parallel
        var tasks = new List<Task>();
        foreach (var webhookUrl in request.WebhookUrls)
        {
            var webhookRequest = new SingleWebhookRequest
            {
                WebhookUrl = webhookUrl,
                SecretKey = request.SecretKey,
                Payload = request.Payload
            };

            tasks.Add(context.CallActivityAsync(
                nameof(SendWebhookActivity),
                webhookRequest));
        }

        await Task.WhenAll(tasks);

        logger.LogInformation("Completed webhook notification orchestration");
    }

    /// <summary>
    /// Activity function that sends a single webhook notification.
    /// </summary>
    [Function(nameof(SendWebhookActivity))]
    public static async Task SendWebhookActivity(
        [ActivityTrigger] SingleWebhookRequest request,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(SendWebhookActivity));
        var httpClientFactory = executionContext.InstanceServices
            .GetRequiredService<IHttpClientFactory>();
        var dbContext = executionContext.InstanceServices
            .GetRequiredService<DataManagerDbContext>();

        var httpClient = httpClientFactory.CreateClient();

        // Create log entry for webhook start
        // NOTE: We perform multiple SaveChanges calls (start, success/failure) to ensure
        // log state is persisted at each stage. This is intentional for durability -
        // if the function crashes or is terminated, we want to capture the log state
        // at that moment rather than losing all logging if the final save fails.
        var log = new Log
        {
            LogType = "Webhook",
            Action = "Send",
            Target = request.WebhookUrl.ToString(),
            Status = "Started",
            StartedAt = DateTimeOffset.UtcNow,
            Details = $"Event: {request.Payload.EventType}, DataSet: {request.Payload.DataSetId}",
            CreatedBy = "system"
        };

        dbContext.Logs.Add(log);
        await dbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true);

        const int maxRetries = 3;
        var retryDelay = TimeSpan.FromSeconds(2);
        string? errorMessage = null;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, request.WebhookUrl)
                {
                    Content = System.Net.Http.Json.JsonContent.Create(request.Payload)
                };

                // Add secret key as custom header if provided
                if (!string.IsNullOrEmpty(request.SecretKey))
                {
                    httpRequest.Headers.Add("X-Webhook-Secret", request.SecretKey);
                }

                var response = await httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation(
                        "Successfully sent webhook to {WebhookUrl}",
                        request.WebhookUrl);
                    
                    // Update log entry for success
                    log.Status = "Success";
                    log.EndedAt = DateTimeOffset.UtcNow;
                    await dbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true);
                    return;
                }
                else
                {
                    errorMessage = $"HTTP {response.StatusCode}: {response.ReasonPhrase}";
                    logger.LogWarning(
                        "Webhook to {WebhookUrl} failed with status {StatusCode} (Attempt: {Attempt}/{MaxRetries})",
                        request.WebhookUrl,
                        response.StatusCode,
                        attempt,
                        maxRetries);
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Exception: {ex.Message}";
                logger.LogWarning(
                    ex,
                    "Exception sending webhook to {WebhookUrl} (Attempt: {Attempt}/{MaxRetries})",
                    request.WebhookUrl,
                    attempt,
                    maxRetries);
            }

            // Wait before retry (except on last attempt)
            if (attempt < maxRetries)
            {
                await Task.Delay(retryDelay * attempt);
            }
        }

        logger.LogError(
            "Failed to send webhook to {WebhookUrl} after {MaxRetries} attempts",
            request.WebhookUrl,
            maxRetries);

        // Update log entry for failure
        log.Status = "Failed";
        log.EndedAt = DateTimeOffset.UtcNow;
        log.ErrorMessage = errorMessage ?? "Failed after all retries";
        await dbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true);
    }
}

/// <summary>
/// Request for sending a single webhook notification.
/// Used internally by the orchestrator to pass data to activity functions.
/// </summary>
public record SingleWebhookRequest
{
    public required Uri WebhookUrl { get; init; }
    public string? SecretKey { get; init; }
    public required WebhookPayload Payload { get; init; }
}
