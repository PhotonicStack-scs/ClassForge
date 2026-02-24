namespace ClassForge.Application.DTOs.GradeSubjectRequirements;

public record UpdateGradeSubjectRequirementRequest(
    int PeriodsPerWeek,
    bool PreferDoublePeriods,
    int MaxPeriodsPerDay,
    bool AllowDoublePeriods);
