using System.Collections.Concurrent;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace DataManager.Application.Core.Common.BackgroundJobs;

/// <summary>
/// Local/in-memory implementation of background job service.
/// Uses a simple queue and background tasks for job execution.
/// Suitable for development and non-Azure environments.
/// </summary>
public class LocalBackgroundJobService : IBackgroundJobService, IDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LocalBackgroundJobService> _logger;
    private readonly ConcurrentQueue<(string JobId, WebhookNotificationRequest Request)> _jobQueue;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _processingTask;

    public LocalBackgroundJobService(
        IHttpClientFactory httpClientFactory,
        ILogger<LocalBackgroundJobService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jobQueue = new ConcurrentQueue<(string, WebhookNotificationRequest)>();
        _cancellationTokenSource = new CancellationTokenSource();
        
        // Start background processing task
        _processingTask = Task.Run(ProcessJobsAsync);
    }

    public Task<string> ScheduleWebhookNotificationAsync(
        WebhookNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var jobId = Guid.NewGuid().ToString();
        _jobQueue.Enqueue((jobId, request));
        
        _logger.LogInformation(
            "Scheduled webhook notification job {JobId} for {Count} webhook(s)",
            jobId,
            request.WebhookUrls.Count);
        
        return Task.FromResult(jobId);
    }

    private async Task ProcessJobsAsync()
    {
        _logger.LogInformation("Background job processor started");

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                if (_jobQueue.TryDequeue(out var job))
                {
                    await ExecuteWebhookNotificationAsync(job.JobId, job.Request);
                }
                else
                {
                    // Wait a bit before checking the queue again
                    await Task.Delay(1000, _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when shutting down
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background job processor");
            }
        }

        _logger.LogInformation("Background job processor stopped");
    }

    private async Task ExecuteWebhookNotificationAsync(string jobId, WebhookNotificationRequest request)
    {
        _logger.LogInformation("Executing webhook notification job {JobId}", jobId);

        var httpClient = _httpClientFactory.CreateClient();
        var tasks = new List<Task>();

        foreach (var webhookUrl in request.WebhookUrls)
        {
            tasks.Add(SendWebhookAsync(httpClient, webhookUrl, request.SecretKey, request.Payload, jobId));
        }

        await Task.WhenAll(tasks);

        _logger.LogInformation("Completed webhook notification job {JobId}", jobId);
    }

    private async Task SendWebhookAsync(
        HttpClient httpClient,
        Uri webhookUrl,
        string? secretKey,
        WebhookPayload payload,
        string jobId)
    {
        const int maxRetries = 3;
        var retryDelay = TimeSpan.FromSeconds(2);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, webhookUrl)
                {
                    Content = JsonContent.Create(payload)
                };

                // Add secret key as custom header if provided
                if (!string.IsNullOrEmpty(secretKey))
                {
                    request.Headers.Add("X-Webhook-Secret", secretKey);
                }

                // Add job ID for tracking
                request.Headers.Add("X-Job-Id", jobId);

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Successfully sent webhook to {WebhookUrl} (Job: {JobId})",
                        webhookUrl,
                        jobId);
                    return;
                }
                else
                {
                    _logger.LogWarning(
                        "Webhook to {WebhookUrl} failed with status {StatusCode} (Job: {JobId}, Attempt: {Attempt}/{MaxRetries})",
                        webhookUrl,
                        response.StatusCode,
                        jobId,
                        attempt,
                        maxRetries);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Exception sending webhook to {WebhookUrl} (Job: {JobId}, Attempt: {Attempt}/{MaxRetries})",
                    webhookUrl,
                    jobId,
                    attempt,
                    maxRetries);
            }

            // Wait before retry (except on last attempt)
            if (attempt < maxRetries)
            {
                await Task.Delay(retryDelay * attempt, _cancellationTokenSource.Token);
            }
        }

        _logger.LogError(
            "Failed to send webhook to {WebhookUrl} after {MaxRetries} attempts (Job: {JobId})",
            webhookUrl,
            maxRetries,
            jobId);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        
        try
        {
            // Use async-friendly wait with cancellation token support
            _processingTask.WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Background job processor did not stop within the timeout period");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error waiting for background job processor to stop");
        }
        
        _cancellationTokenSource.Dispose();
    }
}
