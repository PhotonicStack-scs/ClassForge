using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class Subject : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool RequiresSpecialRoom { get; set; }
    public Guid? SpecialRoomId { get; set; }
    public int MaxPeriodsPerDay { get; set; } = 2;
    public bool AllowDoublePeriods { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Room? SpecialRoom { get; set; }
}
