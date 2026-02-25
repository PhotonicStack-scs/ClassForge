using ClassForge.Application.DTOs.Timetables;

namespace ClassForge.Tests.Unit.Scheduling;

public static class TestDataBuilder
{
    public static SchedulingInput CreateMinimalInput(
        int yearCount = 1, int classesPerYear = 1, int subjectCount = 1,
        int teacherCount = 1, int daysCount = 1, int slotsPerDay = 3,
        int periodsPerWeek = 1)
    {
        var years = Enumerable.Range(1, yearCount).Select(i =>
            new SchedulingYear(Guid.NewGuid(), $"Year {i}", i)).ToList();

        var classes = years.SelectMany(y =>
            Enumerable.Range(0, classesPerYear).Select(i =>
                new SchedulingClass(Guid.NewGuid(), y.Id, $"Class {(char)('A' + i)}", i))).ToList();

        var subjects = Enumerable.Range(1, subjectCount).Select(i =>
            new SchedulingSubject(Guid.NewGuid(), $"Subject {i}", false, null)).ToList();

        var rooms = new List<SchedulingRoom>();

        var schoolDays = Enumerable.Range(1, daysCount).Select(d =>
        {
            var dayId = Guid.NewGuid();
            var slots = Enumerable.Range(1, slotsPerDay).Select(s =>
                new SchedulingTimeSlot(Guid.NewGuid(), dayId, s, false)).ToList();
            return new SchedulingSchoolDay(dayId, d, d, slots);
        }).ToList();

        var teachers = Enumerable.Range(1, teacherCount).Select(i =>
            new SchedulingTeacher(
                Guid.NewGuid(), $"Teacher {i}",
                subjects.SelectMany(s => years.Select(y =>
                    new SchedulingTeacherQualification(s.Id, y.SortOrder, y.SortOrder))).ToList(),
                schoolDays.Select(d => new SchedulingTeacherDayConfig(d.Id, slotsPerDay)).ToList(),
                []
            )).ToList();

        var requirements = years.SelectMany(y =>
            subjects.Select(s =>
                new SchedulingRequirement(Guid.NewGuid(), y.Id, s.Id, periodsPerWeek, false, 2, false))).ToList();

        var yearDayConfigs = years.SelectMany(y =>
            schoolDays.Select(d =>
                new SchedulingYearDayConfig(y.Id, d.Id, slotsPerDay))).ToList();

        return new SchedulingInput(years, classes, subjects, rooms, teachers, schoolDays, requirements, [], yearDayConfigs);
    }
}
