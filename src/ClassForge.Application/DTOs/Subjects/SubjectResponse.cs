namespace ClassForge.Application.DTOs.Subjects;

public record SubjectResponse(
    Guid Id,
    string Name,
    bool RequiresSpecialRoom,
    Guid? SpecialRoomId,
    int MaxPeriodsPerDay,
    bool AllowDoublePeriods);
