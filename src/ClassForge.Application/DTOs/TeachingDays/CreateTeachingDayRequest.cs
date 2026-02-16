namespace ClassForge.Application.DTOs.TeachingDays;

public record CreateTeachingDayRequest(int DayOfWeek, bool IsActive, int SortOrder);
