using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class TeacherSubjectQualificationConfiguration : IEntityTypeConfiguration<TeacherSubjectQualification>
{
    public void Configure(EntityTypeBuilder<TeacherSubjectQualification> builder)
    {
        builder.HasKey(q => q.Id);
        builder.HasIndex(q => new { q.TeacherId, q.SubjectId }).IsUnique();
        builder.HasOne(q => q.Teacher).WithMany(t => t.Qualifications).HasForeignKey(q => q.TeacherId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(q => q.Subject).WithMany().HasForeignKey(q => q.SubjectId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(q => q.MinGrade).WithMany().HasForeignKey(q => q.MinGradeId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(q => q.MaxGrade).WithMany().HasForeignKey(q => q.MaxGradeId).OnDelete(DeleteBehavior.NoAction);
    }
}
