using ClassForge.Application.DTOs.Timetables;
using ClassForge.Scheduling.Constraints;
using ClassForge.Scheduling.Models;
using FluentAssertions;

namespace ClassForge.Tests.Unit.Scheduling;

public class SoftConstraintScorerTests
{
    [Fact]
    public void SC1_SameTeacher_ScoresHigherWhenConsistent()
    {
        var input = TestDataBuilder.CreateMinimalInput();
        var teacherId = input.Teachers[0].Id;
        var subjectId = input.Subjects[0].Id;
        var groupId = input.Groups[0].Id;
        var slot1 = input.TeachingDays[0].TimeSlots[0];
        var slot2 = input.TeachingDays[0].TimeSlots[1];

        var state = new SchedulingState();
        var assigned = new LessonVariable
        {
            GradeId = input.Grades[0].Id,
            SubjectId = subjectId,
            GroupIds = [groupId],
            PeriodIndex = 0,
            Domain = []
        };
        assigned.CurrentAssignment = new Assignment(teacherId, slot1.Id, null);
        state.Variables.Add(assigned);

        var next = new LessonVariable
        {
            GradeId = input.Grades[0].Id,
            SubjectId = subjectId,
            GroupIds = [groupId],
            PeriodIndex = 1,
            Domain = []
        };
        state.Variables.Add(next);

        var sameTeacherScore = SoftConstraintScorer.ScoreAssignment(state, input, next, new Assignment(teacherId, slot2.Id, null));

        var otherTeacherId = Guid.NewGuid();
        var diffTeacherScore = SoftConstraintScorer.ScoreAssignment(state, input, next, new Assignment(otherTeacherId, slot2.Id, null));

        sameTeacherScore.Should().BeGreaterThan(diffTeacherScore);
    }

    [Fact]
    public void QualityScore_PerfectAssignment_Returns_Positive()
    {
        var input = TestDataBuilder.CreateMinimalInput(periodsPerWeek: 1);
        var state = new SchedulingState();
        var variable = new LessonVariable
        {
            GradeId = input.Grades[0].Id,
            SubjectId = input.Subjects[0].Id,
            GroupIds = [input.Groups[0].Id],
            PeriodIndex = 0,
            Domain = []
        };
        variable.CurrentAssignment = new Assignment(input.Teachers[0].Id, input.TeachingDays[0].TimeSlots[0].Id, null);
        state.Variables.Add(variable);

        var score = SoftConstraintScorer.ComputeQualityScore(state, input);
        score.Should().BeGreaterThan(0);
    }
}
