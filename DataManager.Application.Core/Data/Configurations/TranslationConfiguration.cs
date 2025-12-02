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

        builder.Property(e => e.CultureName)
            .IsRequired(false)
            .HasMaxLength(10);

        builder.Property(e => e.Content)
            .IsRequired();

        builder.Property(e => e.ContentTemplate)
            .IsRequired(false);

        // Configure relationship with DataSet
        builder.HasOne(e => e.DataSet)
            .WithMany()
            .HasForeignKey(e => e.DataSetId)
            .OnDelete(DeleteBehavior.SetNull);

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

        // Add indexes for common queries
        builder.HasIndex(e => e.DataSetId);
        builder.HasIndex(e => e.CultureName);
        builder.HasIndex(e => e.LayoutId);
        builder.HasIndex(e => e.SourceId);
        builder.HasIndex(e => new { e.InternalGroupName1, e.InternalGroupName2, e.ResourceName, e.CultureName });

        // Configure relationship with OldVersions
        builder.HasMany(e => e.OldVersions)
            .WithOne()
            .HasForeignKey(e => e.TranslationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
