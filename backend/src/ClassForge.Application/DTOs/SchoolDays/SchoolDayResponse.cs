namespace ClassForge.Application.DTOs.SchoolDays;

public record SchoolDayResponse(Guid Id, int DayOfWeek, string Name, bool IsActive, int SortOrder);
