namespace ClassForge.Application.DTOs.TeacherSlotBlocks;

public record TeacherSlotBlockResponse(
    Guid Id,
    Guid TimeSlotId,
    string? Reason);
