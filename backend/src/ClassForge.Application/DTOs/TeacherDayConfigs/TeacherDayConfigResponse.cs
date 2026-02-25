namespace ClassForge.Application.DTOs.TeacherDayConfigs;

public record TeacherDayConfigResponse(
    Guid Id,
    Guid SchoolDayId,
    int MaxPeriods);
