using ClassForge.Application.DTOs.Timetables;
using ClassForge.Scheduling.Constraints;
using ClassForge.Scheduling.Models;

namespace ClassForge.Scheduling;

public static class ConstraintPropagation
{
    public static SchedulingState BuildInitialState(SchedulingInput input)
    {
        var state = new SchedulingState();
        var variables = new List<LessonVariable>();

        // Build a lookup of all non-break time slots
        var allSlots = input.TeachingDays
            .SelectMany(d => d.TimeSlots)
            .Where(s => !s.IsBreak)
            .ToList();

        // Build qualified teacher lookup: (subjectId, gradeId) -> [teacherIds]
        var qualifiedTeachers = BuildQualifiedTeacherLookup(input);

        // Process combined lessons first (mandatory ones merge groups)
        var processedCombined = new HashSet<(Guid GradeId, Guid SubjectId)>();
        foreach (var combined in input.CombinedLessons.Where(c => c.IsMandatory))
        {
            var requirement = input.Requirements.FirstOrDefault(r =>
                r.GradeId == combined.GradeId && r.SubjectId == combined.SubjectId);
            if (requirement is null) continue;

            processedCombined.Add((combined.GradeId, combined.SubjectId));

            // Create variables for combined groups
            var combinedGroupIds = combined.GroupIds;
            var periodsPerWeek = requirement.PeriodsPerWeek;
            var subject = input.Subjects.First(s => s.Id == combined.SubjectId);
            var useDouble = requirement.PreferDoublePeriods && subject.AllowDoublePeriods;
            var doubleCount = useDouble ? periodsPerWeek / 2 : 0;
            var singleCount = periodsPerWeek - doubleCount * 2;

            for (var i = 0; i < doubleCount; i++)
            {
                variables.Add(CreateVariable(input, combined.GradeId, combined.SubjectId,
                    combinedGroupIds, i, true, combined.Id, qualifiedTeachers, allSlots, subject));
            }
            for (var i = 0; i < singleCount; i++)
            {
                variables.Add(CreateVariable(input, combined.GradeId, combined.SubjectId,
                    combinedGroupIds, doubleCount + i, false, combined.Id, qualifiedTeachers, allSlots, subject));
            }
        }

        // Process individual group requirements
        var groups = input.Groups.ToList();
        foreach (var requirement in input.Requirements)
        {
            var gradeGroups = groups.Where(g => g.GradeId == requirement.GradeId).ToList();
            var subject = input.Subjects.First(s => s.Id == requirement.SubjectId);

            foreach (var group in gradeGroups)
            {
                // Skip if this subject/grade is handled by a mandatory combined lesson
                if (processedCombined.Contains((requirement.GradeId, requirement.SubjectId)))
                    continue;

                var groupIds = new List<Guid> { group.Id };
                var useDouble = requirement.PreferDoublePeriods && subject.AllowDoublePeriods;
                var doubleCount = useDouble ? requirement.PeriodsPerWeek / 2 : 0;
                var singleCount = requirement.PeriodsPerWeek - doubleCount * 2;

                // Check for optional combined lessons
                var optionalCombined = input.CombinedLessons.FirstOrDefault(c =>
                    !c.IsMandatory && c.GradeId == requirement.GradeId &&
                    c.SubjectId == requirement.SubjectId && c.GroupIds.Contains(group.Id));

                Guid? combinedId = optionalCombined?.Id;

                for (var i = 0; i < doubleCount; i++)
                {
                    variables.Add(CreateVariable(input, requirement.GradeId, requirement.SubjectId,
                        groupIds, i, true, combinedId, qualifiedTeachers, allSlots, subject));
                }
                for (var i = 0; i < singleCount; i++)
                {
                    variables.Add(CreateVariable(input, requirement.GradeId, requirement.SubjectId,
                        groupIds, doubleCount + i, false, combinedId, qualifiedTeachers, allSlots, subject));
                }
            }
        }

        state.Variables.AddRange(variables);
        RunArcConsistency(state, input);
        return state;
    }

    private static LessonVariable CreateVariable(
        SchedulingInput input, Guid gradeId, Guid subjectId,
        List<Guid> groupIds, int periodIndex, bool isDouble, Guid? combinedLessonId,
        Dictionary<(Guid SubjectId, Guid GradeId), List<Guid>> qualifiedTeachers,
        List<SchedulingTimeSlot> allSlots, SchedulingSubject subject)
    {
        var variable = new LessonVariable
        {
            GradeId = gradeId,
            SubjectId = subjectId,
            PeriodIndex = periodIndex,
            GroupIds = groupIds.ToList(),
            IsDoublePeriod = isDouble,
            CombinedLessonId = combinedLessonId,
            RequiresSpecialRoom = subject.RequiresSpecialRoom,
            SpecialRoomId = subject.SpecialRoomId,
            MaxPeriodsPerDay = subject.MaxPeriodsPerDay
        };

        // Build initial domain
        var teachers = qualifiedTeachers.GetValueOrDefault((subjectId, gradeId)) ?? [];
        Guid? roomId = subject.RequiresSpecialRoom ? subject.SpecialRoomId : null;

        var domain = new List<Assignment>();
        foreach (var teacherId in teachers)
        {
            var teacher = input.Teachers.First(t => t.Id == teacherId);
            foreach (var slot in allSlots)
            {
                // HC-4: Teacher not blocked
                if (teacher.BlockedSlotIds.Contains(slot.Id)) continue;

                // HC-5: Teacher has a day config for this day
                var dayConfig = teacher.DayConfigs.FirstOrDefault(dc => dc.TeachingDayId == slot.TeachingDayId);
                if (dayConfig is null || dayConfig.MaxPeriods == 0) continue;

                // HC-8: Slot within grade day config
                var gradeDayConfig = input.GradeDayConfigs.FirstOrDefault(c =>
                    c.GradeId == gradeId && c.TeachingDayId == slot.TeachingDayId);
                if (gradeDayConfig is null) continue;

                // Count non-break slots up to this slot number to get position
                var nonBreakPosition = input.TeachingDays
                    .First(d => d.Id == slot.TeachingDayId).TimeSlots
                    .Count(s => !s.IsBreak && s.SlotNumber <= slot.SlotNumber);
                if (nonBreakPosition > gradeDayConfig.MaxPeriods) continue;

                // HC-9: Double periods need a consecutive slot
                if (isDouble)
                {
                    var nextSlot = SchedulingState.FindNextNonBreakSlot(input, slot);
                    if (nextSlot is null) continue;
                    var nextPosition = input.TeachingDays
                        .First(d => d.Id == slot.TeachingDayId).TimeSlots
                        .Count(s => !s.IsBreak && s.SlotNumber <= nextSlot.SlotNumber);
                    if (nextPosition > gradeDayConfig.MaxPeriods) continue;
                    if (teacher.BlockedSlotIds.Contains(nextSlot.Id)) continue;
                }

                domain.Add(new Assignment(teacherId, slot.Id, roomId));
            }
        }

        variable.Domain = domain;
        return variable;
    }

    private static Dictionary<(Guid SubjectId, Guid GradeId), List<Guid>> BuildQualifiedTeacherLookup(SchedulingInput input)
    {
        var lookup = new Dictionary<(Guid, Guid), List<Guid>>();

        foreach (var teacher in input.Teachers)
        {
            foreach (var qual in teacher.Qualifications)
            {
                var qualifiedGrades = input.Grades
                    .Where(g => g.SortOrder >= qual.MinGradeSortOrder && g.SortOrder <= qual.MaxGradeSortOrder);

                foreach (var grade in qualifiedGrades)
                {
                    var key = (qual.SubjectId, grade.Id);
                    if (!lookup.ContainsKey(key))
                        lookup[key] = [];
                    lookup[key].Add(teacher.Id);
                }
            }
        }

        return lookup;
    }

    private static void RunArcConsistency(SchedulingState state, SchedulingInput input)
    {
        // Simple arc consistency: prune domains based on static constraints
        // This is a lightweight pass — the backtracking solver does forward checking too
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var variable in state.Variables)
            {
                var before = variable.Domain.Count;
                variable.Domain = variable.Domain
                    .Where(a => IsStaticallyConsistent(variable, a, input))
                    .ToList();
                if (variable.Domain.Count < before)
                    changed = true;
            }
        }
    }

    private static bool IsStaticallyConsistent(LessonVariable variable, Assignment assignment, SchedulingInput input)
    {
        // HC-7: Special room check
        if (!HardConstraintChecker.CheckSpecialRoom(input, variable.SubjectId, assignment.RoomId))
            return false;

        return true;
    }
}
