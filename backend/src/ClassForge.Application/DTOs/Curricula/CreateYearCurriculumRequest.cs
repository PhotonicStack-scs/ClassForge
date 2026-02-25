namespace ClassForge.Application.DTOs.Curricula;

public record CreateYearCurriculumRequest(
    Guid SubjectId,
    int PeriodsPerWeek,
    bool PreferDoublePeriods,
    int MaxPeriodsPerDay,
    bool AllowDoublePeriods);
