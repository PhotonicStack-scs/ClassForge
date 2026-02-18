using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class TimeSlot : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TeachingDayId { get; set; }
    public int SlotNumber { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsBreak { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public TeachingDay TeachingDay { get; set; } = null!;
}
