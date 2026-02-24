using ClassForge.Domain.Enums;
using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class Timetable : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public TimetableStatus Status { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public decimal? QualityScore { get; set; }
    public Guid CreatedBy { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public List<TimetableEntry> Entries { get; set; } = [];
    public List<TimetableReport> Reports { get; set; } = [];
}
