namespace ClassForge.Application.DTOs.TeacherQualifications;

public record TeacherQualificationResponse(
    Guid Id,
    Guid SubjectId,
    Guid MinGradeId,
    Guid MaxGradeId);
