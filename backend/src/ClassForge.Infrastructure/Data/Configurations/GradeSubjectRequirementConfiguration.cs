using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class GradeSubjectRequirementConfiguration : IEntityTypeConfiguration<GradeSubjectRequirement>
{
    public void Configure(EntityTypeBuilder<GradeSubjectRequirement> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.TenantId, r.GradeId, r.SubjectId }).IsUnique();
        builder.HasOne(r => r.Tenant).WithMany().HasForeignKey(r => r.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.Grade).WithMany(g => g.SubjectRequirements).HasForeignKey(r => r.GradeId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(r => r.Subject).WithMany().HasForeignKey(r => r.SubjectId).OnDelete(DeleteBehavior.NoAction);
    }
}
