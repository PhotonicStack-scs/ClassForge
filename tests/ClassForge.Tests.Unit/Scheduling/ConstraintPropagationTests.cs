using ClassForge.Application.DTOs.Timetables;
using ClassForge.Scheduling;
using FluentAssertions;

namespace ClassForge.Tests.Unit.Scheduling;

public class ConstraintPropagationTests
{
    [Fact]
    public void DomainReduction_BlockedSlotsRemoved()
    {
        var input = TestDataBuilder.CreateMinimalInput(
            gradeCount: 1, groupsPerGrade: 1, subjectCount: 1,
            teacherCount: 1, daysCount: 1, slotsPerDay: 3, periodsPerWeek: 1);

        var blockedSlotId = input.TeachingDays[0].TimeSlots[0].Id;

        // Block the first slot for the teacher
        var teacher = input.Teachers[0];
        var modifiedInput = input with
        {
            Teachers = [teacher with { BlockedSlotIds = [blockedSlotId] }]
        };

        var state = ConstraintPropagation.BuildInitialState(modifiedInput);

        state.Variables.Should().NotBeEmpty();
        foreach (var variable in state.Variables)
        {
            variable.Domain.Should().NotContain(a => a.TimeSlotId == blockedSlotId,
                "blocked slot should be excluded from domain");
        }
    }

    [Fact]
    public void DomainReduction_UnqualifiedTeachersExcluded()
    {
        var input = TestDataBuilder.CreateMinimalInput(
            gradeCount: 1, groupsPerGrade: 1, subjectCount: 1,
            teacherCount: 1, daysCount: 1, slotsPerDay: 3, periodsPerWeek: 1);

        // Add a teacher with no qualifications
        var unqualifiedTeacher = new SchedulingTeacher(
            Guid.NewGuid(), "Unqualified",
            [], // no qualifications
            input.TeachingDays.Select(d => new SchedulingTeacherDayConfig(d.Id, 5)).ToList(),
            []);

        var modifiedInput = input with
        {
            Teachers = [..input.Teachers, unqualifiedTeacher]
        };

        var state = ConstraintPropagation.BuildInitialState(modifiedInput);

        foreach (var variable in state.Variables)
        {
            variable.Domain.Should().NotContain(a => a.TeacherId == unqualifiedTeacher.Id,
                "unqualified teacher should not appear in domain");
        }
    }

    [Fact]
    public void GradeDayConfig_SlotsExceedingMaxPeriods_Excluded()
    {
        var input = TestDataBuilder.CreateMinimalInput(
            gradeCount: 1, groupsPerGrade: 1, subjectCount: 1,
            teacherCount: 1, daysCount: 1, slotsPerDay: 5, periodsPerWeek: 1);

        // Limit to 2 max periods
        var modifiedInput = input with
        {
            GradeDayConfigs = input.GradeDayConfigs
                .Select(gc => gc with { MaxPeriods = 2 }).ToList()
        };

        var state = ConstraintPropagation.BuildInitialState(modifiedInput);

        var allSlots = modifiedInput.TeachingDays.SelectMany(d => d.TimeSlots).ToList();
        foreach (var variable in state.Variables)
        {
            // Domain should only contain assignments to the first 2 slots
            foreach (var assignment in variable.Domain)
            {
                var slot = allSlots.First(s => s.Id == assignment.TimeSlotId);
                slot.SlotNumber.Should().BeLessThanOrEqualTo(2,
                    "slots beyond MaxPeriods should be excluded");
            }
        }
    }
}
