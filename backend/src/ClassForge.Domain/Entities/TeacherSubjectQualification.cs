using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class TeacherSubjectQualification : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid MinYearId { get; set; }
    public Guid MaxYearId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Teacher Teacher { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public Year MinYear { get; set; } = null!;
    public Year MaxYear { get; set; } = null!;
}
