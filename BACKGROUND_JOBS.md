# Background Jobs Service

This document describes the background jobs service and how to use it to send webhook notifications.

## Overview

The background jobs service provides an abstraction for executing background tasks, with support for both local development (in-memory queue) and production Azure environments (Durable Functions).

## Architecture

### Interface: `IBackgroundJobService`

Located in `DataManager.Application.Core/Common/BackgroundJobs/IBackgroundJobService.cs`

The main interface for scheduling background jobs:

```csharp
public interface IBackgroundJobService
{
    Task<string> ScheduleWebhookNotificationAsync(
        WebhookNotificationRequest request,
        CancellationToken cancellationToken = default);
}
```

### Implementations

1. **LocalBackgroundJobService** (Default)
   - Located in `DataManager.Application.Core/Common/BackgroundJobs/LocalBackgroundJobService.cs`
   - Uses an in-memory queue and background tasks
   - Suitable for development and non-Azure environments
   - Automatically registered in `ServiceCollectionExtensions`

2. **DurableFunctionsBackgroundJobService** (Azure Production)
   - Located in `DataManager.Host.AzFuncAPI/BackgroundJobs/DurableFunctionsBackgroundJobService.cs`
   - Uses Azure Durable Functions for reliable, scalable execution
   - Enabled via configuration: `BackgroundJobs:UseDurableFunctions = true`
   - Requires Azure storage account for Durable Functions state

## Webhook Notifications

### WebhookNotificationHelper

The `WebhookNotificationHelper` service provides a convenient way to send webhook notifications when TranslationsSet data changes.

**Service Location:** `DataManager.Application.Core/Common/BackgroundJobs/WebhookNotificationHelper.cs`

#### Usage Example

```csharp
public class SaveTranslationCommandHandler : IRequestHandler<SaveTranslationCommand, Guid>
{
    private readonly DataManagerDbContext _context;
    private readonly WebhookNotificationHelper _webhookHelper;

    public SaveTranslationCommandHandler(
        DataManagerDbContext context,
        WebhookNotificationHelper webhookHelper)
    {
        _context = context;
        _webhookHelper = webhookHelper;
    }

    public async Task<Guid> Handle(SaveTranslationCommand request, CancellationToken cancellationToken)
    {
        // ... your save logic ...
        
        // Notify webhooks that the translation was updated
        await _webhookHelper.NotifyTranslationsSetChangeAsync(
            translationsSetId: request.TranslationsSetId,
            eventType: "translation.updated",
            additionalData: new Dictionary<string, object>
            {
                { "resourceName", request.ResourceName },
                { "translationName", request.TranslationName },
                { "cultures", request.Translations.Keys.ToList() }
            },
            cancellationToken: cancellationToken
        );
        
        return translationId;
    }
}
```

### Webhook Payload Structure

Webhooks receive a POST request with the following JSON payload:

```json
{
  "eventType": "translation.updated",
  "timestamp": "2024-12-06T22:00:00Z",
  "translationsSetId": "550e8400-e29b-41d4-a716-446655440000",
  "data": {
    "resourceName": "MyApp.Strings",
    "translationName": "WelcomeMessage",
    "cultures": ["en-US", "pl-PL"]
  }
}
```

### Webhook Request Headers

- `X-Webhook-Secret`: The secret key from the TranslationsSet configuration (if configured)
- `X-Job-Id`: Unique identifier for the background job (for tracking/debugging)
- `Content-Type`: `application/json`

### Event Types

Common event types you can use:

- `translation.updated` - A translation was created or updated
- `translation.deleted` - A translation was deleted
- `translationset.changed` - General changes to a TranslationsSet
- `translations.imported` - Bulk import of translations completed

You can define custom event types as needed for your application.

## Configuration

### Local Development (Default)

No configuration needed. The `LocalBackgroundJobService` is automatically registered and used.

### Azure Production

To enable Durable Functions in Azure:

1. Add configuration in `appsettings.json` or Azure App Settings:

```json
{
  "BackgroundJobs": {
    "UseDurableFunctions": true
  }
}
```

2. Ensure Azure Storage connection string is configured (required by Durable Functions):

```json
{
  "AzureWebJobsStorage": "DefaultEndpointsProtocol=https;AccountName=..."
}
```

## Advanced Usage

### Direct Use of IBackgroundJobService

If you need more control, you can inject `IBackgroundJobService` directly:

```csharp
public class MyCommandHandler : IRequestHandler<MyCommand, Result>
{
    private readonly IBackgroundJobService _backgroundJobService;

    public MyCommandHandler(IBackgroundJobService backgroundJobService)
    {
        _backgroundJobService = backgroundJobService;
    }

    public async Task<Result> Handle(MyCommand request, CancellationToken cancellationToken)
    {
        // Create custom webhook request
        var webhookRequest = new WebhookNotificationRequest
        {
            WebhookUrls = new List<Uri> { new Uri("https://example.com/webhook") },
            SecretKey = "my-secret-key",
            Payload = new WebhookPayload
            {
                EventType = "custom.event",
                TranslationsSetId = request.TranslationsSetId,
                Data = new Dictionary<string, object>
                {
                    { "customField", "customValue" }
                }
            }
        };

        var jobId = await _backgroundJobService.ScheduleWebhookNotificationAsync(
            webhookRequest, 
            cancellationToken);
        
        return new Result { JobId = jobId };
    }
}
```

## Retry Logic

Both implementations include automatic retry logic:

- **Maximum retries**: 3 attempts
- **Retry delay**: Exponential backoff (2s, 4s, 6s)
- **Failure handling**: Logged but doesn't throw exceptions (fire-and-forget pattern)

## Logging

The service logs important events:

- Job scheduling
- Webhook sending attempts
- Success/failure of webhook deliveries
- Retry attempts

Check application logs for troubleshooting webhook delivery issues.

## Testing Webhooks

For testing webhook notifications during development, you can use tools like:

- [webhook.site](https://webhook.site) - Free webhook testing service
- [ngrok](https://ngrok.com) - Expose local server to internet for webhook testing
- [Postman](https://www.postman.com) - Mock server for webhook testing

## TranslationsSet Configuration

To configure webhooks for a TranslationsSet, set these properties:

- `WebhookUrls`: List of URLs to notify (e.g., `["https://example.com/webhook"]`)
- `SecretKey`: Optional secret key for authentication (sent in `X-Webhook-Secret` header)

Example:

```csharp
var translationsSet = new TranslationsSet
{
    Name = "My App Translations",
    WebhookUrls = new List<Uri>
    {
        new Uri("https://api.example.com/webhooks/translations")
    },
    SecretKey = "my-secure-secret-key-12345"
};
```

## Security Considerations

1. **Secret Keys**: Always use secret keys to verify webhook authenticity
2. **HTTPS Only**: Only configure HTTPS webhook URLs in production
3. **Validate Secrets**: Webhook receivers should validate the `X-Webhook-Secret` header
4. **Rate Limiting**: Consider implementing rate limiting on webhook receivers
5. **Idempotency**: Webhook receivers should handle duplicate notifications (use job ID for deduplication)
