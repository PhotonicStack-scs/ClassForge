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
        builder.HasOne(c => c.Year).WithMany().HasForeignKey(c => c.YearId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(c => c.Subject).WithMany().HasForeignKey(c => c.SubjectId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class CombinedLessonClassConfiguration : IEntityTypeConfiguration<CombinedLessonClass>
{
    public void Configure(EntityTypeBuilder<CombinedLessonClass> builder)
    {
        builder.HasKey(c => new { c.CombinedLessonConfigId, c.ClassId });
        builder.HasOne(c => c.CombinedLessonConfig).WithMany(cfg => cfg.Classes).HasForeignKey(c => c.CombinedLessonConfigId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Class).WithMany().HasForeignKey(c => c.ClassId).OnDelete(DeleteBehavior.NoAction);
    }
}
