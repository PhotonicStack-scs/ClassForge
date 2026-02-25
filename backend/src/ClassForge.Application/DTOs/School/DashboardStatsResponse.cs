namespace ClassForge.Application.DTOs.School;

public record DashboardStatsResponse(
    int YearCount,
    int ClassCount,
    int TeacherCount,
    int SubjectCount,
    int RoomCount,
    int TimetableCount,
    Guid? PublishedTimetableId);
