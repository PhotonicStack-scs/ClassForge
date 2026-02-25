namespace ClassForge.Application.DTOs.TeacherQualifications;

public record CreateTeacherQualificationRequest(
    Guid SubjectId,
    Guid MinYearId,
    Guid MaxYearId);
