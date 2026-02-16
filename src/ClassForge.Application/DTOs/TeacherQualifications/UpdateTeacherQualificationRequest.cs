namespace ClassForge.Application.DTOs.TeacherQualifications;

public record UpdateTeacherQualificationRequest(
    Guid SubjectId,
    Guid MinGradeId,
    Guid MaxGradeId);
