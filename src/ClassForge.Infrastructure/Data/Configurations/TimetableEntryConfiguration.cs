using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class TimetableEntryConfiguration : IEntityTypeConfiguration<TimetableEntry>
{
    public void Configure(EntityTypeBuilder<TimetableEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.TimetableId, e.TimeSlotId });
        builder.HasOne(e => e.Timetable).WithMany(t => t.Entries).HasForeignKey(e => e.TimetableId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.TimeSlot).WithMany().HasForeignKey(e => e.TimeSlotId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(e => e.Subject).WithMany().HasForeignKey(e => e.SubjectId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(e => e.Teacher).WithMany().HasForeignKey(e => e.TeacherId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(e => e.Room).WithMany().HasForeignKey(e => e.RoomId).OnDelete(DeleteBehavior.NoAction);
    }
}
