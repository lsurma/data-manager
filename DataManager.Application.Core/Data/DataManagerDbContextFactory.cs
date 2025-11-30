using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DataManager.Application.Core.Data;

public class DataManagerDbContextFactory : IDesignTimeDbContextFactory<DataManagerDbContext>
{
    public DataManagerDbContext CreateDbContext(string[] args)
    {
        // Build configuration to read from local.settings.json or appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DataManagerDb")
            ?? "Data Source=db/DataManager.db";

        var optionsBuilder = new DbContextOptionsBuilder<DataManagerDbContext>();
        optionsBuilder.UseSqlite(connectionString);

        return new DataManagerDbContext(optionsBuilder.Options);
    }
}
