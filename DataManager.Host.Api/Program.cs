using DataManager.Application.Core.Common;
using DataManager.Application.Core.Extensions;
using DataManager.Authentication.Core;
using DataManager.Host.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

var connectionString = builder.Configuration.GetConnectionString("DataManagerDb") ?? "Data Source=db/DataManager.db";
builder.Services.AddDataManagerCore(connectionString, authOptions =>
{
    var rootUsers = builder.Configuration.GetSection("Authorization:RootUsers").Get<string[]>();
    if (rootUsers != null)
    {
        foreach (var userId in rootUsers)
        {
            authOptions.AddRootUser(userId);
        }
    }
});

builder.Services.AddDataManagerAuthentication(builder.Configuration);
builder.Services.AddSingleton<RequestRegistry>();
builder.Services.AddSingleton<ITranslationExporter, CsvExporterService>();
builder.Services.AddSingleton<ITranslationExporter, ExcelExporterService>();
builder.Services.AddSingleton<TranslationExporterFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.Services.InitializeDatabaseAsync();

app.Run();
