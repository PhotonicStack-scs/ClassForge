namespace ClassForge.Application.DTOs.YearDayConfigs;

public record YearDayConfigResponse(
    Guid Id,
    Guid YearId,
    Guid SchoolDayId,
    int MaxPeriods);
