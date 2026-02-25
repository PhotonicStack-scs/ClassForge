using ClassForge.Application.DTOs.Timetables;

namespace ClassForge.Scheduling.Models;

public class LessonVariable
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid YearId { get; init; }
    public Guid SubjectId { get; init; }
    public int PeriodIndex { get; init; }
    public List<Guid> ClassIds { get; init; } = [];
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
    public Dictionary<Guid, HashSet<Guid>> ClassSlotUsage { get; } = new();
    public Dictionary<Guid, HashSet<Guid>> RoomSlotUsage { get; } = new();
    public Dictionary<(Guid TeacherId, Guid SchoolDayId), int> TeacherDailyCount { get; } = new();
    public Dictionary<(Guid ClassId, Guid SubjectId, Guid SchoolDayId), int> ClassSubjectDailyCount { get; } = new();

    public void RecordAssignment(LessonVariable variable, Assignment assignment, SchedulingInput input)
    {
        variable.CurrentAssignment = assignment;

        var slot = FindSlot(input, assignment.TimeSlotId);

        // Teacher slot usage
        if (!TeacherSlotUsage.ContainsKey(assignment.TeacherId))
            TeacherSlotUsage[assignment.TeacherId] = [];
        TeacherSlotUsage[assignment.TeacherId].Add(assignment.TimeSlotId);

        // Class slot usage
        foreach (var classId in variable.ClassIds)
        {
            if (!ClassSlotUsage.ContainsKey(classId))
                ClassSlotUsage[classId] = [];
            ClassSlotUsage[classId].Add(assignment.TimeSlotId);

            var dayKey = (classId, variable.SubjectId, slot.SchoolDayId);
            ClassSubjectDailyCount[dayKey] = ClassSubjectDailyCount.GetValueOrDefault(dayKey) + 1;
        }

        // Room slot usage
        if (assignment.RoomId is { } roomId)
        {
            if (!RoomSlotUsage.ContainsKey(roomId))
                RoomSlotUsage[roomId] = [];
            RoomSlotUsage[roomId].Add(assignment.TimeSlotId);
        }

        // Teacher daily count
        var teacherDayKey = (assignment.TeacherId, slot.SchoolDayId);
        TeacherDailyCount[teacherDayKey] = TeacherDailyCount.GetValueOrDefault(teacherDayKey) + 1;

        // Double period: also record the next slot
        if (variable.IsDoublePeriod)
        {
            var nextSlot = FindNextNonBreakSlot(input, slot);
            if (nextSlot is not null)
            {
                TeacherSlotUsage[assignment.TeacherId].Add(nextSlot.Id);
                foreach (var classId in variable.ClassIds)
                    ClassSlotUsage[classId].Add(nextSlot.Id);
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
        foreach (var classId in variable.ClassIds)
        {
            ClassSlotUsage[classId].Remove(assignment.TimeSlotId);
            var dayKey = (classId, variable.SubjectId, slot.SchoolDayId);
            if (ClassSubjectDailyCount.ContainsKey(dayKey))
                ClassSubjectDailyCount[dayKey]--;
        }
        if (assignment.RoomId is { } roomId)
            RoomSlotUsage[roomId].Remove(assignment.TimeSlotId);

        var teacherDayKey = (assignment.TeacherId, slot.SchoolDayId);
        TeacherDailyCount[teacherDayKey] = TeacherDailyCount[teacherDayKey] - 1;

        if (variable.IsDoublePeriod)
        {
            var nextSlot = FindNextNonBreakSlot(input, slot);
            if (nextSlot is not null)
            {
                TeacherSlotUsage[assignment.TeacherId].Remove(nextSlot.Id);
                foreach (var classId in variable.ClassIds)
                    ClassSlotUsage[classId].Remove(nextSlot.Id);
                if (assignment.RoomId is { } rid)
                    RoomSlotUsage[rid].Remove(nextSlot.Id);
                TeacherDailyCount[teacherDayKey] = TeacherDailyCount[teacherDayKey] - 1;
            }
        }
    }

    private static SchedulingTimeSlot FindSlot(SchedulingInput input, Guid slotId)
    {
        return input.SchoolDays.SelectMany(d => d.TimeSlots).First(s => s.Id == slotId);
    }

    public static SchedulingTimeSlot? FindNextNonBreakSlot(SchedulingInput input, SchedulingTimeSlot currentSlot)
    {
        var day = input.SchoolDays.First(d => d.Id == currentSlot.SchoolDayId);
        return day.TimeSlots
            .Where(s => !s.IsBreak && s.SlotNumber > currentSlot.SlotNumber)
            .OrderBy(s => s.SlotNumber)
            .FirstOrDefault();
    }
}
