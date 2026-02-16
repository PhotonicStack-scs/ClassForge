using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(g => new { g.TenantId, g.GradeId, g.Name }).IsUnique();
        builder.HasOne(g => g.Tenant).WithMany().HasForeignKey(g => g.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(g => g.Grade).WithMany(gr => gr.Groups).HasForeignKey(g => g.GradeId).OnDelete(DeleteBehavior.NoAction);
    }
}
