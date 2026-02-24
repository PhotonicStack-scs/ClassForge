using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class TimetableReportConfiguration : IEntityTypeConfiguration<TimetableReport>
{
    public void Configure(EntityTypeBuilder<TimetableReport> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(r => r.Category).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Message).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.RelatedEntityType).HasMaxLength(100);
        builder.HasOne(r => r.Timetable).WithMany(t => t.Reports).HasForeignKey(r => r.TimetableId).OnDelete(DeleteBehavior.Cascade);
    }
}
