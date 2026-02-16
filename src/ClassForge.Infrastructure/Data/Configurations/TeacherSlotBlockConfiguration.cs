using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class TeacherSlotBlockConfiguration : IEntityTypeConfiguration<TeacherSlotBlock>
{
    public void Configure(EntityTypeBuilder<TeacherSlotBlock> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Reason).HasMaxLength(500);
        builder.HasIndex(b => new { b.TeacherId, b.TimeSlotId }).IsUnique();
        builder.HasOne(b => b.Teacher).WithMany(t => t.BlockedSlots).HasForeignKey(b => b.TeacherId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(b => b.TimeSlot).WithMany().HasForeignKey(b => b.TimeSlotId).OnDelete(DeleteBehavior.NoAction);
    }
}
