using DataManager.Application.Contracts;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using DataManager.Application.Core.Modules.TranslationSet;
using DataManager.Application.Core.Modules.ProjectInstance;
using DataManager.Application.Core.Modules.Translations;
using DataManager.Authentication.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace DataManager.Application.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataManagerCore(
        this IServiceCollection services,
        string connectionString,
        Action<AuthorizationOptions>? configureAuthorization = null)
    {
        services.AddDbContext<DataManagerDbContext>(options =>
            options
                .UseSqlite(connectionString)
                .EnableSensitiveDataLogging()
        );

        // Register MediatR with pipeline behaviors
        // Order matters: TransactionBehavior wraps everything in a transaction first,
        // then LoggingBehavior logs the operation (within the transaction)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(IRequestSender).Assembly);
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.RegisterGenericHandlers = true;
        });

        // Register user context (populated by middleware in Azure Functions)
        services.AddScoped<UserContext>();

        // Register current user service
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register authorization service with options
        var authOptions = new AuthorizationOptions();
        configureAuthorization?.Invoke(authOptions);
        services.AddSingleton(authOptions);
        services.AddScoped<IAuthorizationService, AuthorizationService>();

        // Register culture service
        services.AddSingleton<ICultureService, CultureService>();

        // Register entity-specific query services
        services.AddScoped<IQueryService<TranslationSet, Guid>, TranslationSetsQueryService>();
        services.AddScoped<IQueryService<ProjectInstance, Guid>, ProjectInstancesQueryService>();
        services.AddScoped<IQueryService<Translation, Guid>, TranslationsQueryService>();

        // Also register specialized query services directly for injection when needed
        services.AddScoped<TranslationSetsQueryService>();
        services.AddScoped<TranslationsQueryService>();
        services.AddScoped<ProjectInstancesQueryService>();

        services.AddSingleton<IFilterHandlerRegistry, FilterHandlerRegistry>();

        // Register all filter handlers
        RegisterFilterHandlers(services);


        return services;
    }
    
    private static void RegisterFilterHandlers(IServiceCollection services)
    {
        // Use Scrutor to scan and register all filter handlers
        services.Scan(scan => scan
            .FromAssemblyOf<IFilterHandlerRegistry>()
            .AddClasses(classes => classes.AssignableTo(typeof(IFilterHandler<,>)))
            .AsSelf()
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
        );
    }


    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataManagerDbContext>();

        // Ensure the database directory exists before EF Core tries to create the file
        EnsureDatabaseDirectoryExists(context);

        await context.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(context);
    }

    private static void EnsureDatabaseDirectoryExists(DataManagerDbContext context)
    {
        var connectionString = context.Database.GetConnectionString();
        if (string.IsNullOrEmpty(connectionString))
            return;

        var builder = new SqliteConnectionStringBuilder(connectionString);
        var dataSource = builder.DataSource;

        if (string.IsNullOrEmpty(dataSource))
            return;

        var directory = Path.GetDirectoryName(dataSource);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}