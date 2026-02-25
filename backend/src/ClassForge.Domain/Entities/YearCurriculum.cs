using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class YearCurriculum : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid YearId { get; set; }
    public Guid SubjectId { get; set; }
    public int PeriodsPerWeek { get; set; }
    public bool PreferDoublePeriods { get; set; }
    public int MaxPeriodsPerDay { get; set; } = 2;
    public bool AllowDoublePeriods { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Year Year { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
}
