using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class TeacherSubjectQualification : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid MinGradeId { get; set; }
    public Guid MaxGradeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Teacher Teacher { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public Grade MinGrade { get; set; } = null!;
    public Grade MaxGrade { get; set; } = null!;
}
