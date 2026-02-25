namespace ClassForge.Application.DTOs.Curricula;

public record UpdateYearCurriculumRequest(
    int PeriodsPerWeek,
    bool PreferDoublePeriods,
    int MaxPeriodsPerDay,
    bool AllowDoublePeriods);
