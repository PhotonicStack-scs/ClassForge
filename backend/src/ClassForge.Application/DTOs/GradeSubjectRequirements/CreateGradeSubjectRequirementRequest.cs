namespace ClassForge.Application.DTOs.GradeSubjectRequirements;

public record CreateGradeSubjectRequirementRequest(
    Guid SubjectId,
    int PeriodsPerWeek,
    bool PreferDoublePeriods,
    int MaxPeriodsPerDay,
    bool AllowDoublePeriods);
