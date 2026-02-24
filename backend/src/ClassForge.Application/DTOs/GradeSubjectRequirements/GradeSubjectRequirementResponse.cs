namespace ClassForge.Application.DTOs.GradeSubjectRequirements;

public record GradeSubjectRequirementResponse(
    Guid Id,
    Guid GradeId,
    Guid SubjectId,
    int PeriodsPerWeek,
    bool PreferDoublePeriods,
    int MaxPeriodsPerDay,
    bool AllowDoublePeriods,
    string SubjectName);
