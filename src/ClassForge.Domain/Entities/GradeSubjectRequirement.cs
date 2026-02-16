using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class GradeSubjectRequirement : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid GradeId { get; set; }
    public Guid SubjectId { get; set; }
    public int PeriodsPerWeek { get; set; }
    public bool PreferDoublePeriods { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Grade Grade { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
}
