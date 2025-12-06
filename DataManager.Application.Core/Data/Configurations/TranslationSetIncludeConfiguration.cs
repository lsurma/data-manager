using DataManager.Application.Core.Modules.TranslationSet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataManager.Application.Core.Data.Configurations;

public class TranslationSetIncludeConfiguration : IEntityTypeConfiguration<TranslationSetInclude>
{
    public void Configure(EntityTypeBuilder<TranslationSetInclude> builder)
    {
        builder.HasKey(e => new { e.ParentTranslationSetId, e.IncludedTranslationSetId });

        builder.Property(e => e.CreatedAt)
            .HasConversion(
                v => v.UtcDateTime.Ticks,
                v => new DateTimeOffset(v, TimeSpan.Zero));

        // Prevent circular references (a dataset cannot include itself)
        builder.HasIndex(e => new { e.ParentTranslationSetId, e.IncludedTranslationSetId })
            .IsUnique();
    }
}
