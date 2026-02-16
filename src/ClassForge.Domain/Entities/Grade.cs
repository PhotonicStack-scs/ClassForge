using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class Grade : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<Group> Groups { get; set; } = [];
    public ICollection<GradeSubjectRequirement> SubjectRequirements { get; set; } = [];
    public ICollection<GradeDayConfig> DayConfigs { get; set; } = [];
}
