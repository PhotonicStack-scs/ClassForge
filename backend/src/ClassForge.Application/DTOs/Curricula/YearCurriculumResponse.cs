namespace ClassForge.Application.DTOs.Curricula;

public record YearCurriculumResponse(
    Guid Id,
    Guid YearId,
    Guid SubjectId,
    int PeriodsPerWeek,
    bool PreferDoublePeriods,
    int MaxPeriodsPerDay,
    bool AllowDoublePeriods,
    string SubjectName);
