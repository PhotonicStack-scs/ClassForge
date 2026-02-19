namespace ClassForge.Application.DTOs.TeachingDays;

public record TeachingDayResponse(Guid Id, int DayOfWeek, string Name, bool IsActive, int SortOrder);
