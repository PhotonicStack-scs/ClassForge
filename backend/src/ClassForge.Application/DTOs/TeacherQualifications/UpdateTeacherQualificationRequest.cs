namespace ClassForge.Application.DTOs.TeacherQualifications;

public record UpdateTeacherQualificationRequest(
    Guid SubjectId,
    Guid MinYearId,
    Guid MaxYearId);
