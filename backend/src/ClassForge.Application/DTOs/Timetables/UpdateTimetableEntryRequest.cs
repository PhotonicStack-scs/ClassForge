namespace ClassForge.Application.DTOs.Timetables;

public record UpdateTimetableEntryRequest(
    Guid TimeSlotId,
    Guid SubjectId,
    Guid TeacherId,
    Guid? RoomId,
    bool IsDoublePeriod,
    List<Guid> GroupIds);
