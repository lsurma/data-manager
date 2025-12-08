using DataManager.Application.Core.Abstractions;
using DataManager.Application.Core.Modules.ProjectInstance;
using DataManager.Application.Core.Modules.Translations;
using DataManager.Application.Core.Modules.Log;
using DataManager.Application.Core.Modules.DataSets;
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

    public DbSet<Log> Logs { get; set; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">
    /// Indicates whether AcceptAllChanges() is called after the changes have been sent successfully to the database.
    /// Additionally, when false and a transaction is open, the save operation is skipped entirely (returns 0).
    /// This allows TransactionBehavior to control when changes are persisted.
    /// Pass true to force save even within a transaction (can also be thought of as forceSave).
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        // If there's an open transaction and acceptAllChangesOnSuccess is false, do nothing
        // This allows TransactionBehavior to control when changes are persisted
        if (Database.CurrentTransaction != null && !acceptAllChangesOnSuccess)
        {
            return Task.FromResult(0);
        }

        SetAuditFields();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // NOTE: Defaults to acceptAllChangesOnSuccess: false to skip saves within open transactions.
        // This is intentional design to work with MediatR's TransactionBehavior, which manages
        // when changes should be persisted. To force save within a transaction, explicitly
        // call SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken).
        return SaveChangesAsync(acceptAllChangesOnSuccess: false, cancellationToken);
    }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">
    /// Indicates whether AcceptAllChanges() is called after the changes have been sent successfully to the database.
    /// Additionally, when false and a transaction is open, the save operation is skipped entirely (returns 0).
    /// This allows TransactionBehavior to control when changes are persisted.
    /// Pass true to force save even within a transaction (can also be thought of as forceSave).
    /// </param>
    /// <returns>The number of state entries written to the database.</returns>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        // If there's an open transaction and acceptAllChangesOnSuccess is false, do nothing
        // This allows TransactionBehavior to control when changes are persisted
        if (Database.CurrentTransaction != null && !acceptAllChangesOnSuccess)
        {
            return 0;
        }

        SetAuditFields();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override int SaveChanges()
    {
        // NOTE: Defaults to acceptAllChangesOnSuccess: false to skip saves within open transactions.
        // This is intentional design to work with MediatR's TransactionBehavior, which manages
        // when changes should be persisted. To force save within a transaction, explicitly
        // call SaveChanges(acceptAllChangesOnSuccess: true).
        return SaveChanges(acceptAllChangesOnSuccess: false);
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

        // Set TranslationKey for Translation entities
        SetTranslationKeys();
    }

    private void SetTranslationKeys()
    {
        var translationEntries = ChangeTracker.Entries<Translation>();

        foreach (var entry in translationEntries)
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                var translation = entry.Entity;
                translation.TranslationKey = $"{translation.ResourceName}_{translation.TranslationName}";
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataManagerDbContext).Assembly);
    }
}
