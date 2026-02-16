using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class GradeDayConfigConfiguration : IEntityTypeConfiguration<GradeDayConfig>
{
    public void Configure(EntityTypeBuilder<GradeDayConfig> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => new { c.TenantId, c.GradeId, c.TeachingDayId }).IsUnique();
        builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Grade).WithMany(g => g.DayConfigs).HasForeignKey(c => c.GradeId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(c => c.TeachingDay).WithMany().HasForeignKey(c => c.TeachingDayId).OnDelete(DeleteBehavior.NoAction);
    }
}
