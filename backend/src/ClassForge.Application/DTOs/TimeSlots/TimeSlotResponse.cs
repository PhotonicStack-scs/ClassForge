namespace ClassForge.Application.DTOs.TimeSlots;

public record TimeSlotResponse(
    Guid Id,
    Guid SchoolDayId,
    int SlotNumber,
    string StartTime,
    string EndTime,
    bool IsBreak);
