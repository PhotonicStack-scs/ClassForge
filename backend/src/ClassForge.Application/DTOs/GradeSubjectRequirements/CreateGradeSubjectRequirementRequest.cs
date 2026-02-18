namespace ClassForge.Application.DTOs.GradeSubjectRequirements;

public record CreateGradeSubjectRequirementRequest(
    Guid SubjectId,
    int PeriodsPerWeek,
    bool PreferDoublePeriods);
