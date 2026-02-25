using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class YearCurriculumConfiguration : IEntityTypeConfiguration<YearCurriculum>
{
    public void Configure(EntityTypeBuilder<YearCurriculum> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.TenantId, r.YearId, r.SubjectId }).IsUnique();
        builder.HasOne(r => r.Tenant).WithMany().HasForeignKey(r => r.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.Year).WithMany(y => y.Curricula).HasForeignKey(r => r.YearId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(r => r.Subject).WithMany().HasForeignKey(r => r.SubjectId).OnDelete(DeleteBehavior.NoAction);
    }
}
