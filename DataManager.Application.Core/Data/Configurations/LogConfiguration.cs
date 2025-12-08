using DataManager.Application.Core.Modules.Log;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataManager.Application.Core.Data.Configurations;

public class LogConfiguration : AuditableEntityConfiguration<Log>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Log> builder)
    {
        builder.Property(e => e.LogType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Target)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.StartedAt)
            .IsRequired();

        builder.Property(e => e.EndedAt);

        builder.Property(e => e.ErrorMessage);

        builder.Property(e => e.Details);

        
        // SQLite workaround: Store DateTimeOffset as UTC ticks for proper sorting/filtering
        builder.Property(e => e.StartedAt)
            .HasConversion(
                v => v.UtcDateTime.Ticks,
                v => new DateTimeOffset(v, TimeSpan.Zero));

        builder.Property(e => e.EndedAt)
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime.Ticks : (long?)null,
                v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : null);
        
        // Add indices for common queries
        builder.HasIndex(e => e.LogType);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.StartedAt);
    }
}
