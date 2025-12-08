using DataManager.AI.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DataManager.AI.Core.Extensions;

/// <summary>
/// Extension methods for registering AI services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers AI services including OpenRouter integration.
    /// </summary>
    public static IServiceCollection AddAIServices(this IServiceCollection services)
    {
        // Register HttpClient for OpenRouterService
        services.AddHttpClient<IOpenRouterService, OpenRouterService>();

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        return services;
    }
}
