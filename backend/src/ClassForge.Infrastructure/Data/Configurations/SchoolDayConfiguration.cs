using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class SchoolDayConfiguration : IEntityTypeConfiguration<SchoolDay>
{
    public void Configure(EntityTypeBuilder<SchoolDay> builder)
    {
        builder.HasKey(d => d.Id);
        builder.HasIndex(d => new { d.TenantId, d.DayOfWeek }).IsUnique();
        builder.HasOne(d => d.Tenant).WithMany().HasForeignKey(d => d.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
