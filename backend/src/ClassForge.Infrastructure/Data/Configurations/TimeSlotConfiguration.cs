using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
{
    public void Configure(EntityTypeBuilder<TimeSlot> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => new { s.SchoolDayId, s.SlotNumber }).IsUnique();
        builder.HasOne(s => s.Tenant).WithMany().HasForeignKey(s => s.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.SchoolDay).WithMany(d => d.TimeSlots).HasForeignKey(s => s.SchoolDayId).OnDelete(DeleteBehavior.NoAction);
    }
}
