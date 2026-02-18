using ClassForge.Application.DTOs.Timetables;

namespace ClassForge.Tests.Unit.Scheduling;

public static class TestDataBuilder
{
    public static SchedulingInput CreateMinimalInput(
        int gradeCount = 1, int groupsPerGrade = 1, int subjectCount = 1,
        int teacherCount = 1, int daysCount = 1, int slotsPerDay = 3,
        int periodsPerWeek = 1)
    {
        var grades = Enumerable.Range(1, gradeCount).Select(i =>
            new SchedulingGrade(Guid.NewGuid(), $"Grade {i}", i)).ToList();

        var groups = grades.SelectMany(g =>
            Enumerable.Range(0, groupsPerGrade).Select(i =>
                new SchedulingGroup(Guid.NewGuid(), g.Id, $"Group {(char)('A' + i)}", i))).ToList();

        var subjects = Enumerable.Range(1, subjectCount).Select(i =>
            new SchedulingSubject(Guid.NewGuid(), $"Subject {i}", false, null, 2, false)).ToList();

        var rooms = new List<SchedulingRoom>();

        var days = Enumerable.Range(1, daysCount).Select(d =>
        {
            var dayId = Guid.NewGuid();
            var slots = Enumerable.Range(1, slotsPerDay).Select(s =>
                new SchedulingTimeSlot(Guid.NewGuid(), dayId, s, false)).ToList();
            return new SchedulingTeachingDay(dayId, d, d, slots);
        }).ToList();

        var teachers = Enumerable.Range(1, teacherCount).Select(i =>
            new SchedulingTeacher(
                Guid.NewGuid(), $"Teacher {i}",
                subjects.SelectMany(s => grades.Select(g =>
                    new SchedulingTeacherQualification(s.Id, g.SortOrder, g.SortOrder))).ToList(),
                days.Select(d => new SchedulingTeacherDayConfig(d.Id, slotsPerDay)).ToList(),
                []
            )).ToList();

        var requirements = grades.SelectMany(g =>
            subjects.Select(s =>
                new SchedulingRequirement(Guid.NewGuid(), g.Id, s.Id, periodsPerWeek, false))).ToList();

        var gradeDayConfigs = grades.SelectMany(g =>
            days.Select(d =>
                new SchedulingGradeDayConfig(g.Id, d.Id, slotsPerDay))).ToList();

        return new SchedulingInput(grades, groups, subjects, rooms, teachers, days, requirements, [], gradeDayConfigs);
    }
}
