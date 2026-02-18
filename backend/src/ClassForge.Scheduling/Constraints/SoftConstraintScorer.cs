using ClassForge.Application.DTOs.Timetables;
using ClassForge.Scheduling.Models;

namespace ClassForge.Scheduling.Constraints;

public static class SoftConstraintScorer
{
    private const int SC1_SameTeacherWeight = 1000;
    private const int SC2_MinimizeGapsWeight = 100;
    private const int SC3_AvoidSameSubjectWeight = 50;
    private const int SC4_EvenDistributionWeight = 20;
    private const int SC5_DoublePeriodWeight = 10;
    private const int SC6_CombinedLessonWeight = 5;

    /// <summary>Score a candidate assignment (higher is better).</summary>
    public static int ScoreAssignment(
        SchedulingState state, SchedulingInput input,
        LessonVariable variable, Assignment candidate)
    {
        var score = 0;
        score += ScoreSameTeacher(state, variable, candidate);
        score += ScoreMinimizeGaps(state, input, candidate);
        score += ScoreAvoidSameSubject(state, input, variable, candidate);
        score += ScoreEvenDistribution(state, input, variable, candidate);
        score += ScoreDoublePeriod(variable);
        score += ScoreCombinedLesson(variable);
        return score;
    }

    /// <summary>SC-1: Prefer same teacher per subject per group (teacher consistency).</summary>
    private static int ScoreSameTeacher(SchedulingState state, LessonVariable variable, Assignment candidate)
    {
        // Check if this teacher is already assigned to the same subject for these groups
        var sameSubjectVars = state.Variables.Where(v =>
            v.IsAssigned &&
            v.SubjectId == variable.SubjectId &&
            v.GroupIds.Intersect(variable.GroupIds).Any());

        if (sameSubjectVars.Any(v => v.CurrentAssignment!.TeacherId == candidate.TeacherId))
            return SC1_SameTeacherWeight;

        // No existing assignment for this subject/group combo — neutral
        if (!sameSubjectVars.Any())
            return SC1_SameTeacherWeight;

        return 0;
    }

    /// <summary>SC-2: Minimize teacher schedule gaps (contiguous blocks preferred).</summary>
    private static int ScoreMinimizeGaps(
        SchedulingState state, SchedulingInput input, Assignment candidate)
    {
        var slot = input.TeachingDays.SelectMany(d => d.TimeSlots).First(s => s.Id == candidate.TimeSlotId);
        var day = input.TeachingDays.First(d => d.Id == slot.TeachingDayId);

        if (!state.TeacherSlotUsage.TryGetValue(candidate.TeacherId, out var usedSlots))
            return SC2_MinimizeGapsWeight;

        var daySlotNumbers = day.TimeSlots
            .Where(s => !s.IsBreak && usedSlots.Contains(s.Id))
            .Select(s => s.SlotNumber)
            .ToList();

        if (daySlotNumbers.Count == 0)
            return SC2_MinimizeGapsWeight;

        // Check adjacency: is this slot adjacent to any existing slot?
        var isAdjacent = daySlotNumbers.Any(n =>
            Math.Abs(n - slot.SlotNumber) == 1 ||
            // Allow gap of 2 if the middle slot is a break
            day.TimeSlots.Any(s => s.IsBreak &&
                ((s.SlotNumber > Math.Min(n, slot.SlotNumber) && s.SlotNumber < Math.Max(n, slot.SlotNumber)))));

        return isAdjacent ? SC2_MinimizeGapsWeight : 0;
    }

    /// <summary>SC-3: Avoid same subject twice in a day (unless it's a double period).</summary>
    private static int ScoreAvoidSameSubject(
        SchedulingState state, SchedulingInput input,
        LessonVariable variable, Assignment candidate)
    {
        if (variable.IsDoublePeriod) return SC3_AvoidSameSubjectWeight;

        var slot = input.TeachingDays.SelectMany(d => d.TimeSlots).First(s => s.Id == candidate.TimeSlotId);

        foreach (var groupId in variable.GroupIds)
        {
            var count = state.GroupSubjectDailyCount.GetValueOrDefault((groupId, variable.SubjectId, slot.TeachingDayId));
            if (count > 0) return 0;
        }

        return SC3_AvoidSameSubjectWeight;
    }

    /// <summary>SC-4: Even distribution of subject across the week.</summary>
    private static int ScoreEvenDistribution(
        SchedulingState state, SchedulingInput input,
        LessonVariable variable, Assignment candidate)
    {
        var slot = input.TeachingDays.SelectMany(d => d.TimeSlots).First(s => s.Id == candidate.TimeSlotId);
        var teachingDayId = slot.TeachingDayId;

        // Count how many periods of this subject are already on each day for these groups
        var dayCounts = new Dictionary<Guid, int>();
        foreach (var day in input.TeachingDays)
        {
            var count = 0;
            foreach (var groupId in variable.GroupIds)
            {
                count += state.GroupSubjectDailyCount.GetValueOrDefault((groupId, variable.SubjectId, day.Id));
            }
            dayCounts[day.Id] = count;
        }

        var currentDayCount = dayCounts.GetValueOrDefault(teachingDayId);
        var minDayCount = dayCounts.Values.DefaultIfEmpty(0).Min();

        // Prefer placing on a day with fewer periods of this subject
        return currentDayCount <= minDayCount ? SC4_EvenDistributionWeight : 0;
    }

    /// <summary>SC-5: Honor double period preference.</summary>
    private static int ScoreDoublePeriod(LessonVariable variable)
    {
        return variable.IsDoublePeriod ? SC5_DoublePeriodWeight : 0;
    }

    /// <summary>SC-6: Utilize combined lessons when configured.</summary>
    private static int ScoreCombinedLesson(LessonVariable variable)
    {
        return variable.CombinedLessonId is not null ? SC6_CombinedLessonWeight : 0;
    }

    /// <summary>Compute total quality score for a completed timetable (0-100 scale).</summary>
    public static decimal ComputeQualityScore(SchedulingState state, SchedulingInput input)
    {
        if (state.Variables.Count == 0) return 100m;

        var totalPossible = state.Variables.Count *
            (SC1_SameTeacherWeight + SC2_MinimizeGapsWeight + SC3_AvoidSameSubjectWeight +
             SC4_EvenDistributionWeight + SC5_DoublePeriodWeight + SC6_CombinedLessonWeight);

        var totalActual = 0;
        foreach (var variable in state.Variables.Where(v => v.IsAssigned))
        {
            totalActual += ScoreAssignment(state, input, variable, variable.CurrentAssignment!);
        }

        if (totalPossible == 0) return 100m;
        return Math.Round((decimal)totalActual / totalPossible * 100, 2);
    }
}
