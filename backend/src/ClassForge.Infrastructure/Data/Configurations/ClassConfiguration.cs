using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(g => new { g.TenantId, g.YearId, g.Name }).IsUnique();
        builder.HasOne(g => g.Tenant).WithMany().HasForeignKey(g => g.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(g => g.Year).WithMany(y => y.Classes).HasForeignKey(g => g.YearId).OnDelete(DeleteBehavior.NoAction);
    }
}
