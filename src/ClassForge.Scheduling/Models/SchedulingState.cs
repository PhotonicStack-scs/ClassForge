using ClassForge.Application.DTOs.Timetables;

namespace ClassForge.Scheduling.Models;

public class LessonVariable
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid GradeId { get; init; }
    public Guid SubjectId { get; init; }
    public int PeriodIndex { get; init; }
    public List<Guid> GroupIds { get; init; } = [];
    public bool IsDoublePeriod { get; init; }
    public Guid? CombinedLessonId { get; init; }
    public bool RequiresSpecialRoom { get; init; }
    public Guid? SpecialRoomId { get; init; }
    public int MaxPeriodsPerDay { get; init; }

    public List<Assignment> Domain { get; set; } = [];
    public Assignment? CurrentAssignment { get; set; }
    public bool IsAssigned => CurrentAssignment is not null;
}

public record Assignment(Guid TeacherId, Guid TimeSlotId, Guid? RoomId);

public class SchedulingState
{
    public List<LessonVariable> Variables { get; init; } = [];
    public Dictionary<Guid, HashSet<Guid>> TeacherSlotUsage { get; } = new();
    public Dictionary<Guid, HashSet<Guid>> GroupSlotUsage { get; } = new();
    public Dictionary<Guid, HashSet<Guid>> RoomSlotUsage { get; } = new();
    public Dictionary<(Guid TeacherId, Guid TeachingDayId), int> TeacherDailyCount { get; } = new();
    public Dictionary<(Guid GroupId, Guid SubjectId, Guid TeachingDayId), int> GroupSubjectDailyCount { get; } = new();

    public void RecordAssignment(LessonVariable variable, Assignment assignment, SchedulingInput input)
    {
        variable.CurrentAssignment = assignment;

        var slot = FindSlot(input, assignment.TimeSlotId);

        // Teacher slot usage
        if (!TeacherSlotUsage.ContainsKey(assignment.TeacherId))
            TeacherSlotUsage[assignment.TeacherId] = [];
        TeacherSlotUsage[assignment.TeacherId].Add(assignment.TimeSlotId);

        // Group slot usage
        foreach (var groupId in variable.GroupIds)
        {
            if (!GroupSlotUsage.ContainsKey(groupId))
                GroupSlotUsage[groupId] = [];
            GroupSlotUsage[groupId].Add(assignment.TimeSlotId);

            var dayKey = (groupId, variable.SubjectId, slot.TeachingDayId);
            GroupSubjectDailyCount[dayKey] = GroupSubjectDailyCount.GetValueOrDefault(dayKey) + 1;
        }

        // Room slot usage
        if (assignment.RoomId is { } roomId)
        {
            if (!RoomSlotUsage.ContainsKey(roomId))
                RoomSlotUsage[roomId] = [];
            RoomSlotUsage[roomId].Add(assignment.TimeSlotId);
        }

        // Teacher daily count
        var teacherDayKey = (assignment.TeacherId, slot.TeachingDayId);
        TeacherDailyCount[teacherDayKey] = TeacherDailyCount.GetValueOrDefault(teacherDayKey) + 1;

        // Double period: also record the next slot
        if (variable.IsDoublePeriod)
        {
            var nextSlot = FindNextNonBreakSlot(input, slot);
            if (nextSlot is not null)
            {
                TeacherSlotUsage[assignment.TeacherId].Add(nextSlot.Id);
                foreach (var groupId in variable.GroupIds)
                    GroupSlotUsage[groupId].Add(nextSlot.Id);
                if (assignment.RoomId is { } rid)
                    RoomSlotUsage[rid].Add(nextSlot.Id);
                TeacherDailyCount[teacherDayKey] = TeacherDailyCount[teacherDayKey] + 1;
            }
        }
    }

    public void UndoAssignment(LessonVariable variable, Assignment assignment, SchedulingInput input)
    {
        variable.CurrentAssignment = null;

        var slot = FindSlot(input, assignment.TimeSlotId);

        TeacherSlotUsage[assignment.TeacherId].Remove(assignment.TimeSlotId);
        foreach (var groupId in variable.GroupIds)
        {
            GroupSlotUsage[groupId].Remove(assignment.TimeSlotId);
            var dayKey = (groupId, variable.SubjectId, slot.TeachingDayId);
            if (GroupSubjectDailyCount.ContainsKey(dayKey))
                GroupSubjectDailyCount[dayKey]--;
        }
        if (assignment.RoomId is { } roomId)
            RoomSlotUsage[roomId].Remove(assignment.TimeSlotId);

        var teacherDayKey = (assignment.TeacherId, slot.TeachingDayId);
        TeacherDailyCount[teacherDayKey] = TeacherDailyCount[teacherDayKey] - 1;

        if (variable.IsDoublePeriod)
        {
            var nextSlot = FindNextNonBreakSlot(input, slot);
            if (nextSlot is not null)
            {
                TeacherSlotUsage[assignment.TeacherId].Remove(nextSlot.Id);
                foreach (var groupId in variable.GroupIds)
                    GroupSlotUsage[groupId].Remove(nextSlot.Id);
                if (assignment.RoomId is { } rid)
                    RoomSlotUsage[rid].Remove(nextSlot.Id);
                TeacherDailyCount[teacherDayKey] = TeacherDailyCount[teacherDayKey] - 1;
            }
        }
    }

    private static SchedulingTimeSlot FindSlot(SchedulingInput input, Guid slotId)
    {
        return input.TeachingDays.SelectMany(d => d.TimeSlots).First(s => s.Id == slotId);
    }

    public static SchedulingTimeSlot? FindNextNonBreakSlot(SchedulingInput input, SchedulingTimeSlot currentSlot)
    {
        var day = input.TeachingDays.First(d => d.Id == currentSlot.TeachingDayId);
        return day.TimeSlots
            .Where(s => !s.IsBreak && s.SlotNumber > currentSlot.SlotNumber)
            .OrderBy(s => s.SlotNumber)
            .FirstOrDefault();
    }
}
