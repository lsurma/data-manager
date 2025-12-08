using DataManager.Application.Contracts;
using DataManager.Host.WA;
using DataManager.Host.WA.DAL;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure authorization with default policy requiring authentication
builder.Services.AddAuthorizationCore(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Configure MSAL authentication
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.LoginMode = "redirect";
    
    // Add default scopes for API access
    // We using .default
    // Dont need to specify each scope individually
    var defaultScopes = builder.Configuration.GetSection("AzureAd:DefaultScopes").Get<string[]>();
    if (defaultScopes != null)
    {
        foreach (var scope in defaultScopes)
        {
            options.ProviderOptions.DefaultAccessTokenScopes.Add(scope);
        }
    }
});

// Configure HttpClient to use the API base URL from configuration
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:7233/api/";
var scopes = builder.Configuration.GetSection("AzureAd:DefaultScopes").Get<string[]>();

// Configure ApiAuthorizationMessageHandler for external API
builder.Services.AddScoped(sp => new ApiAuthorizationMessageHandler(
    sp.GetRequiredService<IAccessTokenProvider>(),
    sp.GetRequiredService<NavigationManager>(),
    sp.GetRequiredService<ILogger<ApiAuthorizationMessageHandler>>(),
    apiBaseUrl,
    scopes
));

// Register DataManagerHttpClient as Typed Client with ApiAuthorizationMessageHandler
builder.Services.AddHttpClient<DataManagerHttpClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<ApiAuthorizationMessageHandler>();



builder.Services.AddScoped<IRequestSender, HttpRequestSender>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<ICookieService, CookieService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<NavigationHelper>();
builder.Services.AddScoped<AppDataContext>();
builder.Services.AddTransient<IKeyboardShortcutsService, KeyboardShortcutsService>();

builder.Services.AddFluentUIComponents();
builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();
