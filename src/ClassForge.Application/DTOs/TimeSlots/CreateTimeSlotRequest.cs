namespace ClassForge.Application.DTOs.TimeSlots;

public record CreateTimeSlotRequest(
    int SlotNumber,
    string StartTime,
    string EndTime,
    bool IsBreak);
