namespace ClassForge.Application.DTOs.GradeDayConfigs;

public record GradeDayConfigResponse(
    Guid Id,
    Guid GradeId,
    Guid TeachingDayId,
    int MaxPeriods);
