using ClassForge.Application.DTOs.Timetables;
using ClassForge.Scheduling.Constraints;
using ClassForge.Scheduling.Models;

namespace ClassForge.Scheduling;

public class BacktrackingSolver
{
    private readonly SchedulingInput _input;
    private readonly SchedulingState _state;
    private readonly IProgress<int>? _progress;
    private readonly CancellationToken _ct;
    private int _assignedCount;
    private readonly int _totalVariables;

    public BacktrackingSolver(SchedulingInput input, SchedulingState state, IProgress<int>? progress, CancellationToken ct)
    {
        _input = input;
        _state = state;
        _progress = progress;
        _ct = ct;
        _totalVariables = state.Variables.Count;
    }

    public bool Solve()
    {
        return Backtrack();
    }

    private bool Backtrack()
    {
        _ct.ThrowIfCancellationRequested();

        // All variables assigned — success
        var unassigned = SelectNextVariable();
        if (unassigned is null) return true;

        // Order domain values by LCV + soft scoring
        var orderedValues = OrderDomainValues(unassigned);

        foreach (var candidate in orderedValues)
        {
            if (!HardConstraintChecker.IsValidAssignment(_state, _input, unassigned, candidate))
                continue;

            // Save domain snapshots for forward checking
            var domainSnapshots = SaveDomainSnapshots(unassigned);

            // Assign
            _state.RecordAssignment(unassigned, candidate, _input);
            _assignedCount++;
            ReportProgress();

            // Forward checking: prune other variable domains
            if (ForwardCheck(unassigned))
            {
                if (Backtrack())
                    return true;
            }

            // Undo
            _state.UndoAssignment(unassigned, candidate, _input);
            _assignedCount--;
            RestoreDomainSnapshots(domainSnapshots, unassigned);
        }

        return false;
    }

    /// <summary>MRV: Pick unassigned variable with smallest domain.
    /// Ties: combined lessons first, then special room subjects.</summary>
    private LessonVariable? SelectNextVariable()
    {
        return _state.Variables
            .Where(v => !v.IsAssigned)
            .OrderBy(v => v.Domain.Count(d => HardConstraintChecker.IsValidAssignment(_state, _input, v, d)))
            .ThenByDescending(v => v.CombinedLessonId.HasValue ? 1 : 0)
            .ThenByDescending(v => v.RequiresSpecialRoom ? 1 : 0)
            .FirstOrDefault();
    }

    /// <summary>LCV + soft scoring: order by fewest eliminations, then soft constraint score.</summary>
    private List<Assignment> OrderDomainValues(LessonVariable variable)
    {
        return variable.Domain
            .Where(a => HardConstraintChecker.IsValidAssignment(_state, _input, variable, a))
            .OrderBy(a => CountEliminations(variable, a))
            .ThenByDescending(a => SoftConstraintScorer.ScoreAssignment(_state, _input, variable, a))
            .ToList();
    }

    private int CountEliminations(LessonVariable variable, Assignment candidate)
    {
        var count = 0;
        var slot = _input.TeachingDays.SelectMany(d => d.TimeSlots).First(s => s.Id == candidate.TimeSlotId);

        foreach (var other in _state.Variables.Where(v => !v.IsAssigned && v.Id != variable.Id))
        {
            foreach (var otherAssignment in other.Domain)
            {
                // Would this candidate eliminate this domain value?
                if (otherAssignment.TimeSlotId == candidate.TimeSlotId)
                {
                    if (otherAssignment.TeacherId == candidate.TeacherId) count++;
                    else if (other.GroupIds.Intersect(variable.GroupIds).Any()) count++;
                    else if (candidate.RoomId is not null && otherAssignment.RoomId == candidate.RoomId) count++;
                }
            }
        }

        return count;
    }

    private bool ForwardCheck(LessonVariable assigned)
    {
        foreach (var other in _state.Variables.Where(v => !v.IsAssigned))
        {
            other.Domain = other.Domain
                .Where(a => HardConstraintChecker.IsValidAssignment(_state, _input, other, a))
                .ToList();

            if (other.Domain.Count == 0)
                return false;
        }
        return true;
    }

    private Dictionary<Guid, List<Assignment>> SaveDomainSnapshots(LessonVariable excludeVar)
    {
        return _state.Variables
            .Where(v => !v.IsAssigned && v.Id != excludeVar.Id)
            .ToDictionary(v => v.Id, v => v.Domain.ToList());
    }

    private void RestoreDomainSnapshots(Dictionary<Guid, List<Assignment>> snapshots, LessonVariable excludeVar)
    {
        foreach (var variable in _state.Variables.Where(v => !v.IsAssigned && v.Id != excludeVar.Id))
        {
            if (snapshots.TryGetValue(variable.Id, out var saved))
                variable.Domain = saved;
        }
    }

    private void ReportProgress()
    {
        if (_totalVariables > 0)
            _progress?.Report(_assignedCount * 100 / _totalVariables);
    }
}
