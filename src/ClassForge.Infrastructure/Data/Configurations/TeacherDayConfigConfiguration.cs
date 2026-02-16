using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class TeacherDayConfigConfiguration : IEntityTypeConfiguration<TeacherDayConfig>
{
    public void Configure(EntityTypeBuilder<TeacherDayConfig> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => new { c.TeacherId, c.TeachingDayId }).IsUnique();
        builder.HasOne(c => c.Teacher).WithMany(t => t.DayConfigs).HasForeignKey(c => c.TeacherId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.TeachingDay).WithMany().HasForeignKey(c => c.TeachingDayId).OnDelete(DeleteBehavior.NoAction);
    }
}
