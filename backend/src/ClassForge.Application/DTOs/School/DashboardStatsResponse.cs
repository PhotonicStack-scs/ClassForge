namespace ClassForge.Application.DTOs.School;

public record DashboardStatsResponse(
    int GradeCount,
    int GroupCount,
    int TeacherCount,
    int SubjectCount,
    int RoomCount,
    int TimetableCount,
    Guid? PublishedTimetableId);
