namespace ClassForge.Application.DTOs.TimeSlots;

public record TimeSlotResponse(
    Guid Id,
    Guid TeachingDayId,
    int SlotNumber,
    string StartTime,
    string EndTime,
    bool IsBreak);
