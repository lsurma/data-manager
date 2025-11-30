using DataManager.Application.Core.Abstractions;
using DataManager.Application.Core.Modules.DataSet;
using DataManager.Application.Core.Modules.ProjectInstance;
using DataManager.Application.Core.Modules.Translations;
using DataManager.Authentication.Core;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Data;

public class DataManagerDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public DataManagerDbContext(
        DbContextOptions<DataManagerDbContext> options,
        ICurrentUserService? currentUserService = null) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<ProjectInstance> ProjectInstances { get; set; }

    public DbSet<DataSet> DataSets { get; set; }

    public DbSet<Translation> Translations { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SetAuditFields();
        return base.SaveChanges();
    }

    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();
        var currentUserId = _currentUserService?.GetUserId() ?? "system";

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                entry.Entity.CreatedBy = currentUserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                entry.Entity.UpdatedBy = currentUserId;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataManagerDbContext).Assembly);
    }
}
