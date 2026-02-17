using ClassForge.Application.DTOs.Timetables;
using ClassForge.Scheduling.Models;

namespace ClassForge.Scheduling.Constraints;

public static class HardConstraintChecker
{
    /// <summary>HC-1: No teacher double-booking at the same time slot.</summary>
    public static bool CheckNoTeacherDoubleBooking(SchedulingState state, Guid teacherId, Guid timeSlotId)
    {
        return !state.TeacherSlotUsage.TryGetValue(teacherId, out var slots) || !slots.Contains(timeSlotId);
    }

    /// <summary>HC-2: No group double-booking at the same time slot.</summary>
    public static bool CheckNoGroupDoubleBooking(SchedulingState state, List<Guid> groupIds, Guid timeSlotId)
    {
        foreach (var groupId in groupIds)
        {
            if (state.GroupSlotUsage.TryGetValue(groupId, out var slots) && slots.Contains(timeSlotId))
                return false;
        }
        return true;
    }

    /// <summary>HC-3: No room over-capacity (room not already in use at this slot).</summary>
    public static bool CheckRoomAvailable(SchedulingState state, Guid? roomId, Guid timeSlotId)
    {
        if (roomId is null) return true;
        return !state.RoomSlotUsage.TryGetValue(roomId.Value, out var slots) || !slots.Contains(timeSlotId);
    }

    /// <summary>HC-4: Teacher is not blocked at this slot.</summary>
    public static bool CheckTeacherNotBlocked(SchedulingInput input, Guid teacherId, Guid timeSlotId)
    {
        var teacher = input.Teachers.FirstOrDefault(t => t.Id == teacherId);
        return teacher is null || !teacher.BlockedSlotIds.Contains(timeSlotId);
    }

    /// <summary>HC-5: Teacher daily hours not exceeded.</summary>
    public static bool CheckTeacherDailyLimit(
        SchedulingState state, SchedulingInput input,
        Guid teacherId, Guid teachingDayId, int additionalSlots = 1)
    {
        var teacher = input.Teachers.FirstOrDefault(t => t.Id == teacherId);
        if (teacher is null) return false;

        var dayConfig = teacher.DayConfigs.FirstOrDefault(dc => dc.TeachingDayId == teachingDayId);
        if (dayConfig is null) return false;

        var currentCount = state.TeacherDailyCount.GetValueOrDefault((teacherId, teachingDayId));
        return currentCount + additionalSlots <= dayConfig.MaxPeriods;
    }

    /// <summary>HC-6: All required periods must be schedulable (checked at end).</summary>
    public static bool CheckAllPeriodsScheduled(SchedulingState state)
    {
        return state.Variables.All(v => v.IsAssigned);
    }

    /// <summary>HC-7: Special room assigned when subject requires it.</summary>
    public static bool CheckSpecialRoom(SchedulingInput input, Guid subjectId, Guid? roomId)
    {
        var subject = input.Subjects.FirstOrDefault(s => s.Id == subjectId);
        if (subject is null) return true;
        if (!subject.RequiresSpecialRoom) return true;
        return roomId == subject.SpecialRoomId;
    }

    /// <summary>HC-8: Slot within GradeDayConfig.MaxPeriods (slot number not exceeding max allowed).</summary>
    public static bool CheckGradeDayLimit(
        SchedulingInput input, Guid gradeId, Guid teachingDayId, int slotNumber)
    {
        var config = input.GradeDayConfigs.FirstOrDefault(c =>
            c.GradeId == gradeId && c.TeachingDayId == teachingDayId);
        if (config is null) return false;
        return slotNumber <= config.MaxPeriods;
    }

    /// <summary>HC-9: Double periods must use consecutive non-break slots.</summary>
    public static bool CheckDoublePeriodConsecutive(
        SchedulingInput input, SchedulingState state,
        Guid timeSlotId, List<Guid> groupIds, Guid teacherId, Guid? roomId)
    {
        var slot = input.TeachingDays.SelectMany(d => d.TimeSlots).First(s => s.Id == timeSlotId);
        var nextSlot = SchedulingState.FindNextNonBreakSlot(input, slot);
        if (nextSlot is null) return false;

        // Next slot must also be free for teacher, groups, and room
        if (!CheckNoTeacherDoubleBooking(state, teacherId, nextSlot.Id)) return false;
        if (!CheckNoGroupDoubleBooking(state, groupIds, nextSlot.Id)) return false;
        if (!CheckRoomAvailable(state, roomId, nextSlot.Id)) return false;

        return true;
    }

    /// <summary>HC-10: Subject MaxPeriodsPerDay respected.</summary>
    public static bool CheckSubjectDailyLimit(
        SchedulingState state, SchedulingInput input,
        List<Guid> groupIds, Guid subjectId, Guid teachingDayId, int additionalSlots = 1)
    {
        var subject = input.Subjects.FirstOrDefault(s => s.Id == subjectId);
        if (subject is null) return false;

        foreach (var groupId in groupIds)
        {
            var currentCount = state.GroupSubjectDailyCount.GetValueOrDefault((groupId, subjectId, teachingDayId));
            if (currentCount + additionalSlots > subject.MaxPeriodsPerDay)
                return false;
        }
        return true;
    }

    /// <summary>Composite check for a candidate assignment.</summary>
    public static bool IsValidAssignment(
        SchedulingState state, SchedulingInput input,
        LessonVariable variable, Assignment candidate)
    {
        var slot = input.TeachingDays.SelectMany(d => d.TimeSlots).First(s => s.Id == candidate.TimeSlotId);
        var slotsNeeded = variable.IsDoublePeriod ? 2 : 1;

        if (!CheckNoTeacherDoubleBooking(state, candidate.TeacherId, candidate.TimeSlotId)) return false;
        if (!CheckNoGroupDoubleBooking(state, variable.GroupIds, candidate.TimeSlotId)) return false;
        if (!CheckRoomAvailable(state, candidate.RoomId, candidate.TimeSlotId)) return false;
        if (!CheckTeacherNotBlocked(input, candidate.TeacherId, candidate.TimeSlotId)) return false;
        if (!CheckTeacherDailyLimit(state, input, candidate.TeacherId, slot.TeachingDayId, slotsNeeded)) return false;
        if (!CheckSpecialRoom(input, variable.SubjectId, candidate.RoomId)) return false;
        if (!CheckGradeDayLimit(input, variable.GradeId, slot.TeachingDayId, slot.SlotNumber)) return false;
        if (!CheckSubjectDailyLimit(state, input, variable.GroupIds, variable.SubjectId, slot.TeachingDayId, slotsNeeded)) return false;

        if (variable.IsDoublePeriod)
        {
            if (!CheckDoublePeriodConsecutive(input, state, candidate.TimeSlotId, variable.GroupIds, candidate.TeacherId, candidate.RoomId))
                return false;

            // Also check next slot doesn't violate grade day limit
            var nextSlot = SchedulingState.FindNextNonBreakSlot(input, slot);
            if (nextSlot is not null && !CheckGradeDayLimit(input, variable.GradeId, slot.TeachingDayId, nextSlot.SlotNumber))
                return false;

            // Check teacher not blocked at next slot
            if (nextSlot is not null && !CheckTeacherNotBlocked(input, candidate.TeacherId, nextSlot.Id))
                return false;
        }

        return true;
    }
}
