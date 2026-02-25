namespace ClassForge.Application.DTOs.TeacherQualifications;

public record TeacherQualificationResponse(
    Guid Id,
    Guid SubjectId,
    Guid MinYearId,
    Guid MaxYearId,
    string SubjectName,
    string MinYearName,
    string MaxYearName);
