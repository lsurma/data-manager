using DataManager.Application.Core.Modules.TranslationsSet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataManager.Application.Core.Data.Configurations;

public class TranslationsSetIncludeConfiguration : IEntityTypeConfiguration<TranslationsSetInclude>
{
    public void Configure(EntityTypeBuilder<TranslationsSetInclude> builder)
    {
        builder.ToTable("TranslationsSetsIncludes");
        
        builder.HasKey(e => new { e.ParentTranslationsSetId, e.IncludedTranslationsSetId });

        builder.Property(e => e.CreatedAt)
            .HasConversion(
                v => v.UtcDateTime.Ticks,
                v => new DateTimeOffset(v, TimeSpan.Zero));

        // Prevent circular references (a translationsset cannot include itself)
        builder.HasIndex(e => new { e.ParentTranslationsSetId, e.IncludedTranslationsSetId })
            .IsUnique();
    }
}
