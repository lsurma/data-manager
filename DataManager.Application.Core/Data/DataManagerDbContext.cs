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

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="forceSave">When true, saves even if there's an open transaction. When false and a transaction is open, does nothing.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public Task<int> SaveChangesAsync(bool forceSave, CancellationToken cancellationToken = default)
    {
        // If there's an open transaction and forceSave is false, do nothing
        if (Database.CurrentTransaction != null && !forceSave)
        {
            return Task.FromResult(0);
        }

        SetAuditFields();
        return base.SaveChangesAsync(true, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(forceSave: false, cancellationToken);
    }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="forceSave">When true, saves even if there's an open transaction. When false and a transaction is open, does nothing.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public int SaveChanges(bool forceSave)
    {
        // If there's an open transaction and forceSave is false, do nothing
        if (Database.CurrentTransaction != null && !forceSave)
        {
            return 0;
        }

        SetAuditFields();
        return base.SaveChanges(true);
    }

    public override int SaveChanges()
    {
        return SaveChanges(forceSave: false);
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
