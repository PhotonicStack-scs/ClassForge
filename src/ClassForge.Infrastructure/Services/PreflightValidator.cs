using ClassForge.Application.DTOs.Timetables;
using ClassForge.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.Infrastructure.Services;

public class PreflightValidator : IPreflightValidator
{
    private readonly IAppDbContext _db;

    public PreflightValidator(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<PreflightResponse> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var issues = new List<PreflightIssue>();

        await CheckSubjectsWithNoQualifiedTeachers(issues, cancellationToken);
        await CheckTeachersWithZeroAvailableHours(issues, cancellationToken);
        await CheckGradesExceedingAvailableSlots(issues, cancellationToken);
        await CheckCombinedLessonGroupsNotInGrade(issues, cancellationToken);
        await CheckInvalidSubjectReferences(issues, cancellationToken);
        await CheckTeachingDaysWithNoTimeSlots(issues, cancellationToken);
        await CheckRoomCapacityForCombinedLessons(issues, cancellationToken);

        var errorCount = issues.Count(i => i.Severity == "Error");
        var warningCount = issues.Count(i => i.Severity == "Warning");

        return new PreflightResponse(errorCount == 0, errorCount, warningCount, issues);
    }

    private async Task CheckSubjectsWithNoQualifiedTeachers(List<PreflightIssue> issues, CancellationToken ct)
    {
        var requirements = await _db.GradeSubjectRequirements
            .Include(r => r.Grade)
            .Include(r => r.Subject)
            .ToListAsync(ct);

        var qualifications = await _db.TeacherSubjectQualifications
            .Include(q => q.MinGrade)
            .Include(q => q.MaxGrade)
            .ToListAsync(ct);

        foreach (var req in requirements)
        {
            var hasQualifiedTeacher = qualifications.Any(q =>
                q.SubjectId == req.SubjectId &&
                q.MinGrade.SortOrder <= req.Grade.SortOrder &&
                q.MaxGrade.SortOrder >= req.Grade.SortOrder);

            if (!hasQualifiedTeacher)
            {
                issues.Add(new PreflightIssue(
                    "Error",
                    "TeacherCoverage",
                    $"Subject '{req.Subject.Name}' in grade '{req.Grade.Name}' has no qualified teacher.",
                    "GradeSubjectRequirement",
                    req.Id));
            }
        }
    }

    private async Task CheckTeachersWithZeroAvailableHours(List<PreflightIssue> issues, CancellationToken ct)
    {
        var teachers = await _db.Teachers
            .Include(t => t.Qualifications)
            .Include(t => t.DayConfigs)
            .ToListAsync(ct);

        foreach (var teacher in teachers)
        {
            if (teacher.Qualifications.Count == 0)
                continue;

            var totalMaxPeriods = teacher.DayConfigs.Sum(dc => dc.MaxPeriods);
            if (teacher.DayConfigs.Count == 0 || totalMaxPeriods == 0)
            {
                issues.Add(new PreflightIssue(
                    "Warning",
                    "TeacherAvailability",
                    $"Teacher '{teacher.Name}' has qualifications but zero available hours.",
                    "Teacher",
                    teacher.Id));
            }
        }
    }

    private async Task CheckGradesExceedingAvailableSlots(List<PreflightIssue> issues, CancellationToken ct)
    {
        var grades = await _db.Grades.ToListAsync(ct);
        var requirements = await _db.GradeSubjectRequirements.ToListAsync(ct);
        var dayConfigs = await _db.GradeDayConfigs.ToListAsync(ct);

        foreach (var grade in grades)
        {
            var totalRequired = requirements
                .Where(r => r.GradeId == grade.Id)
                .Sum(r => r.PeriodsPerWeek);

            var totalAvailable = dayConfigs
                .Where(dc => dc.GradeId == grade.Id)
                .Sum(dc => dc.MaxPeriods);

            if (totalRequired > totalAvailable)
            {
                issues.Add(new PreflightIssue(
                    "Error",
                    "SlotCapacity",
                    $"Grade '{grade.Name}' requires {totalRequired} periods/week but only {totalAvailable} slots are available.",
                    "Grade",
                    grade.Id));
            }
        }
    }

    private async Task CheckCombinedLessonGroupsNotInGrade(List<PreflightIssue> issues, CancellationToken ct)
    {
        var configs = await _db.CombinedLessonConfigs
            .Include(c => c.Groups)
            .Include(c => c.Grade)
            .Include(c => c.Subject)
            .ToListAsync(ct);

        var groups = await _db.Groups.ToListAsync(ct);

        foreach (var config in configs)
        {
            foreach (var clg in config.Groups)
            {
                var group = groups.FirstOrDefault(g => g.Id == clg.GroupId);
                if (group is not null && group.GradeId != config.GradeId)
                {
                    issues.Add(new PreflightIssue(
                        "Error",
                        "CombinedLessonConfig",
                        $"Combined lesson for '{config.Subject.Name}' in grade '{config.Grade.Name}' references group '{group.Name}' from a different grade.",
                        "CombinedLessonConfig",
                        config.Id));
                }
            }
        }
    }

    private async Task CheckInvalidSubjectReferences(List<PreflightIssue> issues, CancellationToken ct)
    {
        var subjectIds = (await _db.Subjects.Select(s => s.Id).ToListAsync(ct)).ToHashSet();

        var requirements = await _db.GradeSubjectRequirements
            .Include(r => r.Grade)
            .ToListAsync(ct);

        foreach (var req in requirements)
        {
            if (!subjectIds.Contains(req.SubjectId))
            {
                issues.Add(new PreflightIssue(
                    "Error",
                    "DataConsistency",
                    $"Grade '{req.Grade.Name}' references a non-existent subject (ID: {req.SubjectId}).",
                    "GradeSubjectRequirement",
                    req.Id));
            }
        }
    }

    private async Task CheckTeachingDaysWithNoTimeSlots(List<PreflightIssue> issues, CancellationToken ct)
    {
        var days = await _db.TeachingDays
            .Include(d => d.TimeSlots)
            .Where(d => d.IsActive)
            .ToListAsync(ct);

        foreach (var day in days)
        {
            var nonBreakSlots = day.TimeSlots.Count(s => !s.IsBreak);
            if (nonBreakSlots == 0)
            {
                issues.Add(new PreflightIssue(
                    "Warning",
                    "TimeStructure",
                    $"Teaching day {day.DayOfWeek} is active but has no non-break time slots.",
                    "TeachingDay",
                    day.Id));
            }
        }
    }

    private async Task CheckRoomCapacityForCombinedLessons(List<PreflightIssue> issues, CancellationToken ct)
    {
        var configs = await _db.CombinedLessonConfigs
            .Include(c => c.Subject)
            .Include(c => c.Grade)
            .Where(c => c.Subject.RequiresSpecialRoom && c.Subject.SpecialRoomId != null)
            .ToListAsync(ct);

        var rooms = await _db.Rooms.ToListAsync(ct);

        foreach (var config in configs)
        {
            var room = rooms.FirstOrDefault(r => r.Id == config.Subject.SpecialRoomId);
            if (room is not null && room.Capacity < config.MaxGroupsPerLesson)
            {
                issues.Add(new PreflightIssue(
                    "Error",
                    "RoomCapacity",
                    $"Room '{room.Name}' (capacity {room.Capacity}) cannot fit {config.MaxGroupsPerLesson} groups for combined '{config.Subject.Name}' in grade '{config.Grade.Name}'.",
                    "CombinedLessonConfig",
                    config.Id));
            }
        }
    }
}
