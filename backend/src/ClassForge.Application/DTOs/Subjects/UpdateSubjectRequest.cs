namespace ClassForge.Application.DTOs.Subjects;

public record UpdateSubjectRequest(
    string Name,
    bool RequiresSpecialRoom,
    Guid? SpecialRoomId,
    string? Color);
