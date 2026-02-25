using ClassForge.Application.DTOs.Timetables;
using ClassForge.Scheduling.Models;

namespace ClassForge.Scheduling;

public static class ReportGenerator
{
    public static List<GeneratedReport> Generate(SchedulingState state, SchedulingInput input)
    {
        var reports = new List<GeneratedReport>();

        CheckTeacherSplits(state, input, reports);
        CheckTeacherGaps(state, input, reports);
        CheckSubjectClustering(state, input, reports);
        CheckDoublePeriodNotUsed(state, input, reports);
        CheckCombinedLessonNotUsed(state, input, reports);
        CheckInfeasible(state, reports);

        return reports;
    }

    private static void CheckTeacherSplits(SchedulingState state, SchedulingInput input, List<GeneratedReport> reports)
    {
        // Group assignments by (classIds key, subjectId) and count distinct teachers
        var groupings = state.Variables
            .Where(v => v.IsAssigned)
            .GroupBy(v => (ClassKey: string.Join(",", v.ClassIds.Order()), v.SubjectId));

        foreach (var group in groupings)
        {
            var distinctTeachers = group.Select(v => v.CurrentAssignment!.TeacherId).Distinct().Count();
            if (distinctTeachers > 1)
            {
                var subject = input.Subjects.FirstOrDefault(s => s.Id == group.Key.SubjectId);
                reports.Add(new GeneratedReport(
                    "Warning", "TeacherSplit",
                    $"Subject '{subject?.Name}' is taught by {distinctTeachers} different teachers for the same class.",
                    "Subject", group.Key.SubjectId));
            }
        }
    }

    private static void CheckTeacherGaps(SchedulingState state, SchedulingInput input, List<GeneratedReport> reports)
    {
        foreach (var teacher in input.Teachers)
        {
            foreach (var day in input.SchoolDays)
            {
                var slotsUsed = state.Variables
                    .Where(v => v.IsAssigned && v.CurrentAssignment!.TeacherId == teacher.Id)
                    .Select(v => input.SchoolDays.SelectMany(d => d.TimeSlots).First(s => s.Id == v.CurrentAssignment!.TimeSlotId))
                    .Where(s => s.SchoolDayId == day.Id)
                    .Select(s => s.SlotNumber)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();

                if (slotsUsed.Count < 2) continue;

                for (var i = 1; i < slotsUsed.Count; i++)
                {
                    var gap = slotsUsed[i] - slotsUsed[i - 1];
                    // Check if there are non-break slots in between that are unused
                    var nonBreakBetween = day.TimeSlots
                        .Count(s => !s.IsBreak && s.SlotNumber > slotsUsed[i - 1] && s.SlotNumber < slotsUsed[i]);
                    if (nonBreakBetween > 0)
                    {
                        reports.Add(new GeneratedReport(
                            "Info", "GapInTeacherSchedule",
                            $"Teacher '{teacher.Name}' has a gap on day {day.DayOfWeek} between slots {slotsUsed[i - 1]} and {slotsUsed[i]}.",
                            "Teacher", teacher.Id));
                    }
                }
            }
        }
    }

    private static void CheckSubjectClustering(SchedulingState state, SchedulingInput input, List<GeneratedReport> reports)
    {
        // Check if any class has the same subject on too many days or too clustered
        var groupings = state.Variables
            .Where(v => v.IsAssigned)
            .SelectMany(v => v.ClassIds.Select(cid => new { ClassId = cid, v.SubjectId, v.CurrentAssignment }))
            .GroupBy(x => (x.ClassId, x.SubjectId));

        foreach (var group in groupings)
        {
            var dayIds = group
                .Select(x => input.SchoolDays.SelectMany(d => d.TimeSlots).First(s => s.Id == x.CurrentAssignment!.TimeSlotId).SchoolDayId)
                .ToList();

            var distinctDays = dayIds.Distinct().Count();
            var totalPeriods = dayIds.Count;

            // If more than half the periods are on a single day, flag it
            var maxOnOneDay = dayIds.GroupBy(d => d).Max(g => g.Count());
            if (totalPeriods >= 3 && maxOnOneDay > totalPeriods / 2 + 1)
            {
                var subject = input.Subjects.FirstOrDefault(s => s.Id == group.Key.SubjectId);
                var schoolClass = input.Classes.FirstOrDefault(c => c.Id == group.Key.ClassId);
                reports.Add(new GeneratedReport(
                    "Warning", "SubjectClustering",
                    $"Subject '{subject?.Name}' for class '{schoolClass?.Name}' has {maxOnOneDay}/{totalPeriods} periods on a single day.",
                    "Subject", group.Key.SubjectId));
            }
        }
    }

    private static void CheckDoublePeriodNotUsed(SchedulingState state, SchedulingInput input, List<GeneratedReport> reports)
    {
        var requirements = input.Requirements.Where(r => r.PreferDoublePeriods).ToList();
        foreach (var req in requirements)
        {
            if (!req.AllowDoublePeriods) continue;

            var subject = input.Subjects.First(s => s.Id == req.SubjectId);
            var hasDouble = state.Variables.Any(v =>
                v.IsAssigned && v.SubjectId == req.SubjectId && v.YearId == req.YearId && v.IsDoublePeriod);

            if (!hasDouble)
            {
                reports.Add(new GeneratedReport(
                    "Info", "DoublePeriodNotUsed",
                    $"Subject '{subject.Name}' in year prefers double periods but none were scheduled.",
                    "YearCurriculum", req.Id));
            }
        }
    }

    private static void CheckCombinedLessonNotUsed(SchedulingState state, SchedulingInput input, List<GeneratedReport> reports)
    {
        foreach (var combined in input.CombinedLessons.Where(c => !c.IsMandatory))
        {
            var used = state.Variables.Any(v => v.IsAssigned && v.CombinedLessonId == combined.Id);
            if (!used)
            {
                var subject = input.Subjects.FirstOrDefault(s => s.Id == combined.SubjectId);
                reports.Add(new GeneratedReport(
                    "Info", "CombinedLessonNotUsed",
                    $"Optional combined lesson for '{subject?.Name}' was not utilized.",
                    "CombinedLessonConfig", combined.Id));
            }
        }
    }

    private static void CheckInfeasible(SchedulingState state, List<GeneratedReport> reports)
    {
        var unassigned = state.Variables.Where(v => !v.IsAssigned).ToList();
        foreach (var variable in unassigned)
        {
            reports.Add(new GeneratedReport(
                "Error", "InfeasibleConstraint",
                $"Could not schedule period {variable.PeriodIndex + 1} for subject {variable.SubjectId} (classes: {string.Join(", ", variable.ClassIds)}).",
                "YearCurriculum", null));
        }
    }
}
