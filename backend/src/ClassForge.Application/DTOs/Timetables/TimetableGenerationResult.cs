namespace ClassForge.Application.DTOs.Timetables;

public record TimetableGenerationResult(
    bool Success,
    decimal? QualityScore,
    List<GeneratedEntry> Entries,
    List<GeneratedReport> Reports);

public record GeneratedEntry(
    Guid TimeSlotId,
    Guid SubjectId,
    Guid TeacherId,
    Guid? RoomId,
    bool IsDoublePeriod,
    Guid? CombinedLessonClassId,
    List<Guid> ClassIds);

public record GeneratedReport(
    string Type,
    string Category,
    string Message,
    string? RelatedEntityType,
    Guid? RelatedEntityId);
