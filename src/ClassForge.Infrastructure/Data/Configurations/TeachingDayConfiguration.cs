using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class TeachingDayConfiguration : IEntityTypeConfiguration<TeachingDay>
{
    public void Configure(EntityTypeBuilder<TeachingDay> builder)
    {
        builder.HasKey(d => d.Id);
        builder.HasIndex(d => new { d.TenantId, d.DayOfWeek }).IsUnique();
        builder.HasOne(d => d.Tenant).WithMany().HasForeignKey(d => d.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
