using DataManager.Application.Core.Modules.DataSets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataManager.Application.Core.Data.Configurations;

public class DataSetIncludeConfiguration : IEntityTypeConfiguration<DataSetInclude>
{
    public void Configure(EntityTypeBuilder<DataSetInclude> builder)
    {
        builder.ToTable("DataSetsIncludes");
        
        builder.HasKey(e => new { e.ParentDataSetId, e.IncludedDataSetId });

        builder.Property(e => e.CreatedAt)
            .HasConversion(
                v => v.UtcDateTime.Ticks,
                v => new DateTimeOffset(v, TimeSpan.Zero));

        // Prevent circular references (a dataset cannot include itself)
        builder.HasIndex(e => new { e.ParentDataSetId, e.IncludedDataSetId })
            .IsUnique();
    }
}
