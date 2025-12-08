using DataManager.AI.Core.Extensions;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Common.BackgroundJobs;
using DataManager.Application.Core.Extensions;
using DataManager.Authentication.Core;
using DataManager.Host.AzFuncAPI.BackgroundJobs;
using DataManager.Host.AzFuncAPI.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
var builder = FunctionsApplication.CreateBuilder(args);

// Configure Functions Web Application with middleware pipeline
var functionsApp = builder.ConfigureFunctionsWebApplication();
functionsApp.UseDataManagerAuthentication();

// Add HTTP context accessor for user identity tracking
builder.Services.AddHttpContextAccessor();

// Add AI services
builder.Services.AddAIServices();

// Add database
var connectionString = builder.Configuration.GetConnectionString("DataManagerDb")
    ?? "Data Source=db/DataManager.db";
builder.Services.AddDataManagerCore(connectionString, authOptions =>
{
    // Configure root users from configuration or environment
    var rootUsers = builder.Configuration.GetSection("Authorization:RootUsers").Get<string[]>();
    if (rootUsers != null)
    {
        foreach (var userId in rootUsers)
        {
            authOptions.AddRootUser(userId);
        }
    }

    // For development, you can also hard-code root users here:
    // authOptions.AddRootUser("admin@example.com");
    // authOptions.AddRootUser("api-key-admin");
});

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Add authentication services
builder.Services.AddDataManagerAuthentication(builder.Configuration);

// Find all IRequest from contracts assembly
builder.Services.AddSingleton<RequestRegistry>();
builder.Services.AddSingleton<ITranslationExporter, CsvExporterService>();
builder.Services.AddSingleton<ITranslationExporter, ExcelExporterService>();
builder.Services.AddSingleton<TranslationExporterFactory>();

// Configure background jobs
// Check if we should use Durable Functions (when deployed to Azure) or local implementation
var useDurableFunctions = builder.Configuration.GetValue<bool>("BackgroundJobs:UseDurableFunctions");
if (useDurableFunctions)
{
    // The DurableTaskClient is automatically registered by the Durable Task framework
    // when it detects the [OrchestrationTrigger] and [ActivityTrigger] attributes
    // Replace the default background job service with Durable Functions implementation
    builder.Services.AddSingleton<IBackgroundJobService, DurableFunctionsBackgroundJobService>();
}
// else: Local background job service is already registered in AddDataManagerCore


var app = builder.Build();

// Initialize database
await app.Services.InitializeDatabaseAsync();

app.Run();