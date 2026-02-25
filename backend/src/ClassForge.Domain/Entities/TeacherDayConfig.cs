using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class TeacherDayConfig : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid SchoolDayId { get; set; }
    public int MaxPeriods { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Teacher Teacher { get; set; } = null!;
    public SchoolDay SchoolDay { get; set; } = null!;
}
