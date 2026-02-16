using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
{
    public void Configure(EntityTypeBuilder<TimeSlot> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => new { s.TeachingDayId, s.SlotNumber }).IsUnique();
        builder.HasOne(s => s.Tenant).WithMany().HasForeignKey(s => s.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.TeachingDay).WithMany(d => d.TimeSlots).HasForeignKey(s => s.TeachingDayId).OnDelete(DeleteBehavior.NoAction);
    }
}
