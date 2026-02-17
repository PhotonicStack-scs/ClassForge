using ClassForge.Application.DTOs.Timetables;
using ClassForge.Scheduling.Constraints;
using ClassForge.Scheduling.Models;
using FluentAssertions;

namespace ClassForge.Tests.Unit.Scheduling;

public class HardConstraintTests
{
    [Fact]
    public void HC1_NoTeacherDoubleBooking_RejectsConflict()
    {
        var state = new SchedulingState();
        var teacherId = Guid.NewGuid();
        var slotId = Guid.NewGuid();
        state.TeacherSlotUsage[teacherId] = [slotId];

        HardConstraintChecker.CheckNoTeacherDoubleBooking(state, teacherId, slotId).Should().BeFalse();
    }

    [Fact]
    public void HC1_NoTeacherDoubleBooking_AllowsDifferentSlot()
    {
        var state = new SchedulingState();
        var teacherId = Guid.NewGuid();
        state.TeacherSlotUsage[teacherId] = [Guid.NewGuid()];

        HardConstraintChecker.CheckNoTeacherDoubleBooking(state, teacherId, Guid.NewGuid()).Should().BeTrue();
    }

    [Fact]
    public void HC2_NoGroupDoubleBooking_RejectsConflict()
    {
        var state = new SchedulingState();
        var groupId = Guid.NewGuid();
        var slotId = Guid.NewGuid();
        state.GroupSlotUsage[groupId] = [slotId];

        HardConstraintChecker.CheckNoGroupDoubleBooking(state, [groupId], slotId).Should().BeFalse();
    }

    [Fact]
    public void HC3_RoomAvailable_RejectsConflict()
    {
        var state = new SchedulingState();
        var roomId = Guid.NewGuid();
        var slotId = Guid.NewGuid();
        state.RoomSlotUsage[roomId] = [slotId];

        HardConstraintChecker.CheckRoomAvailable(state, roomId, slotId).Should().BeFalse();
    }

    [Fact]
    public void HC3_RoomAvailable_NullRoomAlwaysValid()
    {
        var state = new SchedulingState();
        HardConstraintChecker.CheckRoomAvailable(state, null, Guid.NewGuid()).Should().BeTrue();
    }

    [Fact]
    public void HC4_TeacherNotBlocked_RejectsBlockedSlot()
    {
        var slotId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var input = TestDataBuilder.CreateMinimalInput();
        var inputWithBlocked = input with
        {
            Teachers = [new SchedulingTeacher(teacherId, "T1", [], [], [slotId])]
        };

        HardConstraintChecker.CheckTeacherNotBlocked(inputWithBlocked, teacherId, slotId).Should().BeFalse();
    }

    [Fact]
    public void HC5_TeacherDailyLimit_RejectsExceeded()
    {
        var state = new SchedulingState();
        var teacherId = Guid.NewGuid();
        var dayId = Guid.NewGuid();
        state.TeacherDailyCount[(teacherId, dayId)] = 5;

        var input = TestDataBuilder.CreateMinimalInput() with
        {
            Teachers = [new SchedulingTeacher(teacherId, "T", [], [new SchedulingTeacherDayConfig(dayId, 5)], [])]
        };

        HardConstraintChecker.CheckTeacherDailyLimit(state, input, teacherId, dayId).Should().BeFalse();
    }

    [Fact]
    public void HC7_SpecialRoom_RejectsWrongRoom()
    {
        var subjectId = Guid.NewGuid();
        var correctRoomId = Guid.NewGuid();
        var wrongRoomId = Guid.NewGuid();

        var input = TestDataBuilder.CreateMinimalInput() with
        {
            Subjects = [new SchedulingSubject(subjectId, "PE", true, correctRoomId, 2, false)]
        };

        HardConstraintChecker.CheckSpecialRoom(input, subjectId, wrongRoomId).Should().BeFalse();
        HardConstraintChecker.CheckSpecialRoom(input, subjectId, correctRoomId).Should().BeTrue();
    }

    [Fact]
    public void HC8_GradeDayLimit_RejectsExceeded()
    {
        var gradeId = Guid.NewGuid();
        var dayId = Guid.NewGuid();

        var input = TestDataBuilder.CreateMinimalInput() with
        {
            GradeDayConfigs = [new SchedulingGradeDayConfig(gradeId, dayId, 3)]
        };

        HardConstraintChecker.CheckGradeDayLimit(input, gradeId, dayId, 3).Should().BeTrue();
        HardConstraintChecker.CheckGradeDayLimit(input, gradeId, dayId, 4).Should().BeFalse();
    }

    [Fact]
    public void HC10_SubjectDailyLimit_RejectsExceeded()
    {
        var state = new SchedulingState();
        var groupId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var dayId = Guid.NewGuid();
        state.GroupSubjectDailyCount[(groupId, subjectId, dayId)] = 2;

        var input = TestDataBuilder.CreateMinimalInput() with
        {
            Subjects = [new SchedulingSubject(subjectId, "Math", false, null, 2, false)]
        };

        HardConstraintChecker.CheckSubjectDailyLimit(state, input, [groupId], subjectId, dayId)
            .Should().BeFalse();
    }
}
