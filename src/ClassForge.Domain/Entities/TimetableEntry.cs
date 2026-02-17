using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class TimetableEntry : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TimetableId { get; set; }
    public Guid TimeSlotId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid? RoomId { get; set; }
    public bool IsDoublePeriod { get; set; }
    public Guid? CombinedLessonGroupId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Timetable Timetable { get; set; } = null!;
    public TimeSlot TimeSlot { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
    public Room? Room { get; set; }
    public List<TimetableEntryGroup> Groups { get; set; } = [];
}
