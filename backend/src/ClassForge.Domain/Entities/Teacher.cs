using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class Teacher : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<TeacherSubjectQualification> Qualifications { get; set; } = [];
    public ICollection<TeacherDayConfig> DayConfigs { get; set; } = [];
    public ICollection<TeacherSlotBlock> BlockedSlots { get; set; } = [];
}
