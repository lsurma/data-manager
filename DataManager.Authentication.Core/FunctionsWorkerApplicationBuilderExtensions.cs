using DataManager.Authentication.Core.Authentication;
using DataManager.Authentication.Core.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataManager.Authentication.Core;

public static class FunctionsWorkerApplicationBuilderExtensions
{
    public static IFunctionsWorkerApplicationBuilder UseDataManagerAuthentication(this IFunctionsWorkerApplicationBuilder builder)
    {
        builder.UseWhen<ApimBypassMiddleware>(context =>
        {
            var settings = context.InstanceServices.GetRequiredService<AuthenticationSettings>();
            return settings.Apim.TrustApim;
        });

        builder.UseMiddleware<FunctionsAuthorizationMiddleware>();

        return builder;
    }
}