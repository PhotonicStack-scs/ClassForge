using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class TeacherSlotBlock : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid TimeSlotId { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Teacher Teacher { get; set; } = null!;
    public TimeSlot TimeSlot { get; set; } = null!;
}
