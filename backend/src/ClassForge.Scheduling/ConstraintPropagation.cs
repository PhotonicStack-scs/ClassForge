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
        var allSlots = input.SchoolDays
            .SelectMany(d => d.TimeSlots)
            .Where(s => !s.IsBreak)
            .ToList();

        // Build qualified teacher lookup: (subjectId, yearId) -> [teacherIds]
        var qualifiedTeachers = BuildQualifiedTeacherLookup(input);

        // Process combined lessons first (mandatory ones merge classes)
        var processedCombined = new HashSet<(Guid YearId, Guid SubjectId)>();
        foreach (var combined in input.CombinedLessons.Where(c => c.IsMandatory))
        {
            var requirement = input.Requirements.FirstOrDefault(r =>
                r.YearId == combined.YearId && r.SubjectId == combined.SubjectId);
            if (requirement is null) continue;

            processedCombined.Add((combined.YearId, combined.SubjectId));

            // Create variables for combined classes
            var combinedClassIds = combined.ClassIds;
            var periodsPerWeek = requirement.PeriodsPerWeek;
            var subject = input.Subjects.First(s => s.Id == combined.SubjectId);
            var useDouble = requirement.PreferDoublePeriods && requirement.AllowDoublePeriods;
            var doubleCount = useDouble ? periodsPerWeek / 2 : 0;
            var singleCount = periodsPerWeek - doubleCount * 2;

            for (var i = 0; i < doubleCount; i++)
            {
                variables.Add(CreateVariable(input, combined.YearId, combined.SubjectId,
                    combinedClassIds, i, true, combined.Id, qualifiedTeachers, allSlots, subject, requirement.MaxPeriodsPerDay));
            }
            for (var i = 0; i < singleCount; i++)
            {
                variables.Add(CreateVariable(input, combined.YearId, combined.SubjectId,
                    combinedClassIds, doubleCount + i, false, combined.Id, qualifiedTeachers, allSlots, subject, requirement.MaxPeriodsPerDay));
            }
        }

        // Process individual class requirements
        var schoolClasses = input.Classes.ToList();
        foreach (var requirement in input.Requirements)
        {
            var yearClasses = schoolClasses.Where(c => c.YearId == requirement.YearId).ToList();
            var subject = input.Subjects.First(s => s.Id == requirement.SubjectId);

            foreach (var schoolClass in yearClasses)
            {
                // Skip if this subject/year is handled by a mandatory combined lesson
                if (processedCombined.Contains((requirement.YearId, requirement.SubjectId)))
                    continue;

                var classIds = new List<Guid> { schoolClass.Id };
                var useDouble = requirement.PreferDoublePeriods && requirement.AllowDoublePeriods;
                var doubleCount = useDouble ? requirement.PeriodsPerWeek / 2 : 0;
                var singleCount = requirement.PeriodsPerWeek - doubleCount * 2;

                // Check for optional combined lessons
                var optionalCombined = input.CombinedLessons.FirstOrDefault(c =>
                    !c.IsMandatory && c.YearId == requirement.YearId &&
                    c.SubjectId == requirement.SubjectId && c.ClassIds.Contains(schoolClass.Id));

                Guid? combinedId = optionalCombined?.Id;

                for (var i = 0; i < doubleCount; i++)
                {
                    variables.Add(CreateVariable(input, requirement.YearId, requirement.SubjectId,
                        classIds, i, true, combinedId, qualifiedTeachers, allSlots, subject, requirement.MaxPeriodsPerDay));
                }
                for (var i = 0; i < singleCount; i++)
                {
                    variables.Add(CreateVariable(input, requirement.YearId, requirement.SubjectId,
                        classIds, doubleCount + i, false, combinedId, qualifiedTeachers, allSlots, subject, requirement.MaxPeriodsPerDay));
                }
            }
        }

        state.Variables.AddRange(variables);
        RunArcConsistency(state, input);
        return state;
    }

    private static LessonVariable CreateVariable(
        SchedulingInput input, Guid yearId, Guid subjectId,
        List<Guid> classIds, int periodIndex, bool isDouble, Guid? combinedLessonId,
        Dictionary<(Guid SubjectId, Guid YearId), List<Guid>> qualifiedTeachers,
        List<SchedulingTimeSlot> allSlots, SchedulingSubject subject, int maxPeriodsPerDay)
    {
        var variable = new LessonVariable
        {
            YearId = yearId,
            SubjectId = subjectId,
            PeriodIndex = periodIndex,
            ClassIds = classIds.ToList(),
            IsDoublePeriod = isDouble,
            CombinedLessonId = combinedLessonId,
            RequiresSpecialRoom = subject.RequiresSpecialRoom,
            SpecialRoomId = subject.SpecialRoomId,
            MaxPeriodsPerDay = maxPeriodsPerDay
        };

        // Build initial domain
        var teachers = qualifiedTeachers.GetValueOrDefault((subjectId, yearId)) ?? [];
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
                var dayConfig = teacher.DayConfigs.FirstOrDefault(dc => dc.SchoolDayId == slot.SchoolDayId);
                if (dayConfig is null || dayConfig.MaxPeriods == 0) continue;

                // HC-8: Slot within year day config
                var yearDayConfig = input.YearDayConfigs.FirstOrDefault(c =>
                    c.YearId == yearId && c.SchoolDayId == slot.SchoolDayId);
                if (yearDayConfig is null) continue;

                // Count non-break slots up to this slot number to get position
                var nonBreakPosition = input.SchoolDays
                    .First(d => d.Id == slot.SchoolDayId).TimeSlots
                    .Count(s => !s.IsBreak && s.SlotNumber <= slot.SlotNumber);
                if (nonBreakPosition > yearDayConfig.MaxPeriods) continue;

                // HC-9: Double periods need a consecutive slot
                if (isDouble)
                {
                    var nextSlot = SchedulingState.FindNextNonBreakSlot(input, slot);
                    if (nextSlot is null) continue;
                    var nextPosition = input.SchoolDays
                        .First(d => d.Id == slot.SchoolDayId).TimeSlots
                        .Count(s => !s.IsBreak && s.SlotNumber <= nextSlot.SlotNumber);
                    if (nextPosition > yearDayConfig.MaxPeriods) continue;
                    if (teacher.BlockedSlotIds.Contains(nextSlot.Id)) continue;
                }

                domain.Add(new Assignment(teacherId, slot.Id, roomId));
            }
        }

        variable.Domain = domain;
        return variable;
    }

    private static Dictionary<(Guid SubjectId, Guid YearId), List<Guid>> BuildQualifiedTeacherLookup(SchedulingInput input)
    {
        var lookup = new Dictionary<(Guid, Guid), List<Guid>>();

        foreach (var teacher in input.Teachers)
        {
            foreach (var qual in teacher.Qualifications)
            {
                var qualifiedYears = input.Years
                    .Where(y => y.SortOrder >= qual.MinYearSortOrder && y.SortOrder <= qual.MaxYearSortOrder);

                foreach (var year in qualifiedYears)
                {
                    var key = (qual.SubjectId, year.Id);
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
