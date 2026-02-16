using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class CombinedLessonConfig : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid GradeId { get; set; }
    public Guid SubjectId { get; set; }
    public bool IsMandatory { get; set; }
    public int MaxGroupsPerLesson { get; set; } = 2;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Grade Grade { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public ICollection<CombinedLessonGroup> Groups { get; set; } = [];
}
