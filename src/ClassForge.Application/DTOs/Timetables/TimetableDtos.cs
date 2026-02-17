namespace ClassForge.Application.DTOs.Timetables;

public record CreateTimetableRequest(string Name);

public record UpdateTimetableRequest(string Name);

public record TimetableResponse(
    Guid Id, string Name, string Status, DateTime? GeneratedAt,
    decimal? QualityScore, Guid CreatedBy, string? ErrorMessage, DateTime CreatedAt);

public record TimetableEntryResponse(
    Guid Id, Guid TimeSlotId, Guid SubjectId, Guid TeacherId,
    Guid? RoomId, bool IsDoublePeriod, Guid? CombinedLessonGroupId, List<Guid> GroupIds);

public record TimetableReportResponse(
    Guid Id, string Type, string Category, string Message,
    string? RelatedEntityType, Guid? RelatedEntityId);

public record TimetableViewEntry(
    Guid EntryId, int DayOfWeek, int SlotNumber, string SubjectName,
    string TeacherName, string? RoomName, bool IsDoublePeriod, List<string> GroupNames);

public record TimetableViewResponse(
    Guid TimetableId, string ViewType, string ViewName, List<TimetableViewEntry> Entries);
