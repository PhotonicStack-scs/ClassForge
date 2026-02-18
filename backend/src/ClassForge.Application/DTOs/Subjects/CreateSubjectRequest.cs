namespace ClassForge.Application.DTOs.Subjects;

public record CreateSubjectRequest(
    string Name,
    bool RequiresSpecialRoom,
    Guid? SpecialRoomId,
    int MaxPeriodsPerDay,
    bool AllowDoublePeriods);
