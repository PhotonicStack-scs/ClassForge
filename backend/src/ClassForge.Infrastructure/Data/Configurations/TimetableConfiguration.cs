using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class TimetableConfiguration : IEntityTypeConfiguration<Timetable>
{
    public void Configure(EntityTypeBuilder<Timetable> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(t => t.QualityScore).HasPrecision(5, 2);
        builder.Property(t => t.ErrorMessage).HasMaxLength(2000);
        builder.HasOne(t => t.Tenant).WithMany().HasForeignKey(t => t.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.CreatedByUser).WithMany().HasForeignKey(t => t.CreatedBy).OnDelete(DeleteBehavior.NoAction);
    }
}
