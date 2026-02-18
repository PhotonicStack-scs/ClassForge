namespace ClassForge.Application.DTOs.TimeSlots;

public record UpdateTimeSlotRequest(
    int SlotNumber,
    string StartTime,
    string EndTime,
    bool IsBreak);
