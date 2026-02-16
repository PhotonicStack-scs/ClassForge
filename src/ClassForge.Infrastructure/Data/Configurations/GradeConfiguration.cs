using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(g => new { g.TenantId, g.Name }).IsUnique();
        builder.HasOne(g => g.Tenant).WithMany().HasForeignKey(g => g.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
