using DataManager.Application.Core.Modules.TranslationsSet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataManager.Application.Core.Data.Configurations;

public class TranslationsSetConfiguration : AuditableEntityConfiguration<TranslationsSet>
{
    protected override void ConfigureEntity(EntityTypeBuilder<TranslationsSet> builder)
    {
        builder.ToTable("TranslationsSets");
        
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

        // Configure many-to-many relationship through TranslationsSetInclude
        builder.HasMany(e => e.Includes)
            .WithOne(e => e.ParentTranslationsSet)
            .HasForeignKey(e => e.ParentTranslationsSetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.IncludedIn)
            .WithOne(e => e.IncludedTranslationsSet)
            .HasForeignKey(e => e.IncludedTranslationsSetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
