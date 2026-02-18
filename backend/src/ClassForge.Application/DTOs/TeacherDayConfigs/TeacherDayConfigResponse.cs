namespace ClassForge.Application.DTOs.TeacherDayConfigs;

public record TeacherDayConfigResponse(
    Guid Id,
    Guid TeachingDayId,
    int MaxPeriods);
