namespace ClassForge.Application.DTOs.SchoolDays;

public record CreateSchoolDayRequest(int DayOfWeek, bool IsActive, int SortOrder);
