using ClassForge.Application.DTOs.Timetables;
using ClassForge.Application.Interfaces;
using ClassForge.Scheduling.Constraints;
using ClassForge.Scheduling.Models;

namespace ClassForge.Scheduling;

public class TimetableGenerator : ITimetableGenerator
{
    public Task<TimetableGenerationResult> GenerateAsync(
        SchedulingInput input,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // Phase 1: Constraint propagation — build variables with initial domains
        var state = ConstraintPropagation.BuildInitialState(input);

        // Check for empty domains before solving
        var emptyDomain = state.Variables.FirstOrDefault(v => v.Domain.Count == 0);
        if (emptyDomain is not null)
        {
            var reports = ReportGenerator.Generate(state, input);
            reports.Insert(0, new GeneratedReport(
                "Error", "InfeasibleConstraint",
                $"No feasible assignments exist for subject {emptyDomain.SubjectId}, period index {emptyDomain.PeriodIndex}. Check teacher qualifications and time slot configuration.",
                null, null));

            return Task.FromResult(new TimetableGenerationResult(false, null, [], reports));
        }

        // Phase 2: Backtracking search
        var solver = new BacktrackingSolver(input, state, progress, cancellationToken);
        var solved = solver.Solve();

        // Phase 3: Build results
        var entries = state.Variables
            .Where(v => v.IsAssigned)
            .Select(v => new GeneratedEntry(
                v.CurrentAssignment!.TimeSlotId,
                v.SubjectId,
                v.CurrentAssignment.TeacherId,
                v.CurrentAssignment.RoomId,
                v.IsDoublePeriod,
                v.CombinedLessonId,
                v.GroupIds))
            .ToList();

        var generatedReports = ReportGenerator.Generate(state, input);
        var qualityScore = solved ? SoftConstraintScorer.ComputeQualityScore(state, input) : (decimal?)null;

        return Task.FromResult(new TimetableGenerationResult(solved, qualityScore, entries, generatedReports));
    }
}
