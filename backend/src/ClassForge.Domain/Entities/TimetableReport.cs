using ClassForge.Domain.Enums;
using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class TimetableReport : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TimetableId { get; set; }
    public ReportType Type { get; set; }
    public string Category { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Timetable Timetable { get; set; } = null!;
}
