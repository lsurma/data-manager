using DataManager.Application.Core.Modules.DataSets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataManager.Application.Core.Data.Configurations;

public class DataSetConfiguration : AuditableEntityConfiguration<DataSet>
{
    protected override void ConfigureEntity(EntityTypeBuilder<DataSet> builder)
    {
        builder.ToTable("DataSets");
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Notes);

        // Store AllowedIdentityIds as comma-separated string in SQLite
        builder.Property(e => e.AllowedIdentityIds)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            )
            .HasColumnType("TEXT");

        // Store AvailableCultures as comma-separated string in SQLite
        // Empty string means all cultures are available
        builder.Property(e => e.AvailableCultures)
            .HasConversion(
                v => !v.Any() ? string.Empty : string.Join(',', v),
                v => string.IsNullOrWhiteSpace(v) ? new List<string>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            )
            .HasColumnType("TEXT")
            .IsRequired();

        builder.Property(e => e.SecretKey)
            .HasMaxLength(500);

        // Store WebhookUrls as comma-separated string in SQLite
        // Note: Invalid URLs are validated and filtered at the handler level before storage,
        // so the Uri constructor here should not throw in practice. If corrupted data exists
        // in the database, it will throw UriFormatException during deserialization.
        builder.Property(e => e.WebhookUrls)
            .HasConversion(
                v => !v.Any() ? string.Empty : string.Join(',', v.Select(uri => uri.ToString())),
                v => string.IsNullOrWhiteSpace(v) 
                    ? new List<Uri>() 
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(s => new Uri(s))
                        .ToList()
            )
            .HasColumnType("TEXT")
            .IsRequired();

        // Configure many-to-many relationship through DataSetInclude
        builder.HasMany(e => e.Includes)
            .WithOne(e => e.ParentDataSet)
            .HasForeignKey(e => e.ParentDataSetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.IncludedIn)
            .WithOne(e => e.IncludedDataSet)
            .HasForeignKey(e => e.IncludedDataSetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
