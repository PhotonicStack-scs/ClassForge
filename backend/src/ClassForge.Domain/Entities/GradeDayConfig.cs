using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class GradeDayConfig : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid GradeId { get; set; }
    public Guid TeachingDayId { get; set; }
    public int MaxPeriods { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Grade Grade { get; set; } = null!;
    public TeachingDay TeachingDay { get; set; } = null!;
}
