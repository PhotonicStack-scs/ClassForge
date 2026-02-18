using ClassForge.Application.DTOs.Timetables;
using ClassForge.Scheduling;
using FluentAssertions;

namespace ClassForge.Tests.Unit.Scheduling;

public class BacktrackingSolverTests
{
    [Fact]
    public async Task Trivial_OneGroup_OneSubject_Solves()
    {
        var input = TestDataBuilder.CreateMinimalInput(
            gradeCount: 1, groupsPerGrade: 1, subjectCount: 1,
            teacherCount: 1, daysCount: 1, slotsPerDay: 3, periodsPerWeek: 1);

        var generator = new TimetableGenerator();
        var result = await generator.GenerateAsync(input);

        result.Success.Should().BeTrue();
        result.Entries.Should().HaveCount(1);
        result.QualityScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task TwoGroups_SameTeacher_DifferentSlots()
    {
        var input = TestDataBuilder.CreateMinimalInput(
            gradeCount: 1, groupsPerGrade: 2, subjectCount: 1,
            teacherCount: 1, daysCount: 1, slotsPerDay: 3, periodsPerWeek: 1);

        var generator = new TimetableGenerator();
        var result = await generator.GenerateAsync(input);

        result.Success.Should().BeTrue();
        result.Entries.Should().HaveCount(2);

        // Same teacher assigned to both
        result.Entries.Select(e => e.TeacherId).Distinct().Should().HaveCount(1);

        // Different time slots
        result.Entries.Select(e => e.TimeSlotId).Distinct().Should().HaveCount(2);
    }

    [Fact]
    public async Task CombinedLesson_MergesGroups()
    {
        var input = TestDataBuilder.CreateMinimalInput(
            gradeCount: 1, groupsPerGrade: 2, subjectCount: 1,
            teacherCount: 1, daysCount: 1, slotsPerDay: 3, periodsPerWeek: 1);

        var combinedInput = input with
        {
            CombinedLessons = [new SchedulingCombinedLesson(
                Guid.NewGuid(), input.Grades[0].Id, input.Subjects[0].Id,
                true, 2, input.Groups.Select(g => g.Id).ToList())]
        };

        var generator = new TimetableGenerator();
        var result = await generator.GenerateAsync(combinedInput);

        result.Success.Should().BeTrue();
        // Combined lesson should create 1 entry (not 2)
        result.Entries.Should().HaveCount(1);
        result.Entries[0].GroupIds.Should().HaveCount(2);
    }

    [Fact]
    public async Task Infeasible_DetectedProperly()
    {
        // 2 groups, 1 teacher, 1 slot — can't schedule both
        var input = TestDataBuilder.CreateMinimalInput(
            gradeCount: 1, groupsPerGrade: 2, subjectCount: 1,
            teacherCount: 1, daysCount: 1, slotsPerDay: 1, periodsPerWeek: 1);

        var generator = new TimetableGenerator();
        var result = await generator.GenerateAsync(input);

        result.Success.Should().BeFalse();
        result.Reports.Should().Contain(r => r.Category == "InfeasibleConstraint");
    }
}
