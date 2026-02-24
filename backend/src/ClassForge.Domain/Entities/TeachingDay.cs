using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class TeachingDay : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public int DayOfWeek { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<TimeSlot> TimeSlots { get; set; } = [];
}
