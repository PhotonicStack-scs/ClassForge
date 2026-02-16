namespace ClassForge.Application.DTOs.TeachingDays;

public record TeachingDayResponse(Guid Id, int DayOfWeek, bool IsActive, int SortOrder);
