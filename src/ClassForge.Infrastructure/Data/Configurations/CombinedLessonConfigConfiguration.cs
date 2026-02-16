using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class CombinedLessonConfigConfiguration : IEntityTypeConfiguration<CombinedLessonConfig>
{
    public void Configure(EntityTypeBuilder<CombinedLessonConfig> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Grade).WithMany().HasForeignKey(c => c.GradeId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(c => c.Subject).WithMany().HasForeignKey(c => c.SubjectId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class CombinedLessonGroupConfiguration : IEntityTypeConfiguration<CombinedLessonGroup>
{
    public void Configure(EntityTypeBuilder<CombinedLessonGroup> builder)
    {
        builder.HasKey(g => new { g.CombinedLessonConfigId, g.GroupId });
        builder.HasOne(g => g.CombinedLessonConfig).WithMany(c => c.Groups).HasForeignKey(g => g.CombinedLessonConfigId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(g => g.Group).WithMany().HasForeignKey(g => g.GroupId).OnDelete(DeleteBehavior.NoAction);
    }
}
