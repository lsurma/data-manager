using DataManager.Application.Core.Modules.Translations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataManager.Application.Core.Data.Configurations;

public class TranslationConfiguration : AuditableEntityConfiguration<Translation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Translation> builder)
    {
        builder.Property(e => e.InternalGroupName1)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.Property(e => e.InternalGroupName2)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.Property(e => e.ResourceName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.TranslationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.TranslationKey)
            .IsRequired()
            .HasMaxLength(400); // ResourceName (200) + TranslationName (200)

        builder.Property(e => e.CultureName)
            .IsRequired(false)
            .HasMaxLength(10);

        builder.Property(e => e.Content)
            .IsRequired();

        builder.Property(e => e.ContentTemplate)
            .IsRequired(false);

        builder.Property(e => e.ContentUpdatedAt)
            .IsRequired(false);

        // Configure relationship with DataSet
        builder.HasOne(e => e.DataSet)
            .WithMany()
            .HasForeignKey(e => e.DataSetId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure self-referencing relationship for SourceTranslation (materialization tracking)
        builder.HasOne(e => e.SourceTranslation)
            .WithMany()
            .HasForeignKey(e => e.SourceTranslationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(e => e.SourceTranslationLastSyncedAt)
            .IsRequired(false);

        // Configure self-referencing relationship for Layout
        builder.HasOne(e => e.Layout)
            .WithMany()
            .HasForeignKey(e => e.LayoutId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure self-referencing relationship for Source
        builder.HasOne(e => e.Source)
            .WithMany()
            .HasForeignKey(e => e.SourceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure version control fields
        builder.Property(e => e.IsCurrentVersion)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.IsDraftVersion)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.IsOldVersion)
            .IsRequired()
            .HasDefaultValue(false);

        // Configure self-referencing relationship for OriginalTranslation (version history)
        builder.HasOne(e => e.OriginalTranslation)
            .WithMany()
            .HasForeignKey(e => e.OriginalTranslationId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.Property(e => e.ContentUpdatedAt)
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime.Ticks : (long?)null,
                v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : null);

        // Add indexes for common queries
        builder.HasIndex(e => e.DataSetId);
        builder.HasIndex(e => e.SourceTranslationId);
        builder.HasIndex(e => e.CultureName);
        builder.HasIndex(e => e.LayoutId);
        builder.HasIndex(e => e.SourceId);
        builder.HasIndex(e => e.OriginalTranslationId);
        builder.HasIndex(e => e.TranslationKey); // Lookup key index
        builder.HasIndex(e => new { e.IsCurrentVersion, e.IsDraftVersion, e.IsOldVersion });
        builder.HasIndex(e => new { e.InternalGroupName1, e.InternalGroupName2, e.ResourceName, e.CultureName });
    }
}
