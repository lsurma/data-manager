using DataManager.Application.Core.Modules.TranslationSet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataManager.Application.Core.Data.Configurations;

public class TranslationSetConfiguration : AuditableEntityConfiguration<TranslationSet>
{
    protected override void ConfigureEntity(EntityTypeBuilder<TranslationSet> builder)
    {
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
        // Null means all cultures are available
        builder.Property(e => e.AvailableCultures)
            .HasConversion(
                v => v == null || !v.Any() ? null : string.Join(',', v),
                v => string.IsNullOrWhiteSpace(v) ? null : v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            )
            .HasColumnType("TEXT")
            .IsRequired(false);

        // Configure many-to-many relationship through TranslationSetInclude
        builder.HasMany(e => e.Includes)
            .WithOne(e => e.ParentTranslationSet)
            .HasForeignKey(e => e.ParentTranslationSetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.IncludedIn)
            .WithOne(e => e.IncludedTranslationSet)
            .HasForeignKey(e => e.IncludedTranslationSetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
