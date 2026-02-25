using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class YearDayConfigConfiguration : IEntityTypeConfiguration<YearDayConfig>
{
    public void Configure(EntityTypeBuilder<YearDayConfig> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => new { c.TenantId, c.YearId, c.SchoolDayId }).IsUnique();
        builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Year).WithMany(y => y.DayConfigs).HasForeignKey(c => c.YearId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(c => c.SchoolDay).WithMany().HasForeignKey(c => c.SchoolDayId).OnDelete(DeleteBehavior.NoAction);
    }
}
