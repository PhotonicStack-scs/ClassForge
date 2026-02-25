using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class YearDayConfig : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid YearId { get; set; }
    public Guid SchoolDayId { get; set; }
    public int MaxPeriods { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Year Year { get; set; } = null!;
    public SchoolDay SchoolDay { get; set; } = null!;
}
