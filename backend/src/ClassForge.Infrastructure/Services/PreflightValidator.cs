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
        await CheckYearsExceedingAvailableSlots(issues, cancellationToken);
        await CheckCombinedLessonClassesNotInYear(issues, cancellationToken);
        await CheckInvalidSubjectReferences(issues, cancellationToken);
        await CheckSchoolDaysWithNoTimeSlots(issues, cancellationToken);
        await CheckRoomCapacityForCombinedLessons(issues, cancellationToken);

        var errorCount = issues.Count(i => i.Severity == "Error");
        var warningCount = issues.Count(i => i.Severity == "Warning");

        return new PreflightResponse(errorCount == 0, errorCount, warningCount, issues);
    }

    private async Task CheckSubjectsWithNoQualifiedTeachers(List<PreflightIssue> issues, CancellationToken ct)
    {
        var requirements = await _db.YearCurricula
            .Include(r => r.Year)
            .Include(r => r.Subject)
            .ToListAsync(ct);

        var qualifications = await _db.TeacherSubjectQualifications
            .Include(q => q.MinYear)
            .Include(q => q.MaxYear)
            .ToListAsync(ct);

        foreach (var req in requirements)
        {
            var hasQualifiedTeacher = qualifications.Any(q =>
                q.SubjectId == req.SubjectId &&
                q.MinYear.SortOrder <= req.Year.SortOrder &&
                q.MaxYear.SortOrder >= req.Year.SortOrder);

            if (!hasQualifiedTeacher)
            {
                issues.Add(new PreflightIssue(
                    "Error",
                    "TeacherCoverage",
                    $"Subject '{req.Subject.Name}' in year '{req.Year.Name}' has no qualified teacher.",
                    "YearCurriculum",
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

    private async Task CheckYearsExceedingAvailableSlots(List<PreflightIssue> issues, CancellationToken ct)
    {
        var years = await _db.Years.ToListAsync(ct);
        var requirements = await _db.YearCurricula.ToListAsync(ct);
        var dayConfigs = await _db.YearDayConfigs.ToListAsync(ct);

        foreach (var year in years)
        {
            var totalRequired = requirements
                .Where(r => r.YearId == year.Id)
                .Sum(r => r.PeriodsPerWeek);

            var totalAvailable = dayConfigs
                .Where(dc => dc.YearId == year.Id)
                .Sum(dc => dc.MaxPeriods);

            if (totalRequired > totalAvailable)
            {
                issues.Add(new PreflightIssue(
                    "Error",
                    "SlotCapacity",
                    $"Year '{year.Name}' requires {totalRequired} periods/week but only {totalAvailable} slots are available.",
                    "Year",
                    year.Id));
            }
        }
    }

    private async Task CheckCombinedLessonClassesNotInYear(List<PreflightIssue> issues, CancellationToken ct)
    {
        var configs = await _db.CombinedLessonConfigs
            .Include(c => c.Classes)
            .Include(c => c.Year)
            .Include(c => c.Subject)
            .ToListAsync(ct);

        var classes = await _db.Classes.ToListAsync(ct);

        foreach (var config in configs)
        {
            foreach (var clc in config.Classes)
            {
                var schoolClass = classes.FirstOrDefault(c => c.Id == clc.ClassId);
                if (schoolClass is not null && schoolClass.YearId != config.YearId)
                {
                    issues.Add(new PreflightIssue(
                        "Error",
                        "CombinedLessonConfig",
                        $"Combined lesson for '{config.Subject.Name}' in year '{config.Year.Name}' references class '{schoolClass.Name}' from a different year.",
                        "CombinedLessonConfig",
                        config.Id));
                }
            }
        }
    }

    private async Task CheckInvalidSubjectReferences(List<PreflightIssue> issues, CancellationToken ct)
    {
        var subjectIds = (await _db.Subjects.Select(s => s.Id).ToListAsync(ct)).ToHashSet();

        var requirements = await _db.YearCurricula
            .Include(r => r.Year)
            .ToListAsync(ct);

        foreach (var req in requirements)
        {
            if (!subjectIds.Contains(req.SubjectId))
            {
                issues.Add(new PreflightIssue(
                    "Error",
                    "DataConsistency",
                    $"Year '{req.Year.Name}' references a non-existent subject (ID: {req.SubjectId}).",
                    "YearCurriculum",
                    req.Id));
            }
        }
    }

    private async Task CheckSchoolDaysWithNoTimeSlots(List<PreflightIssue> issues, CancellationToken ct)
    {
        var days = await _db.SchoolDays
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
                    $"School day {day.DayOfWeek} is active but has no non-break time slots.",
                    "SchoolDay",
                    day.Id));
            }
        }
    }

    private async Task CheckRoomCapacityForCombinedLessons(List<PreflightIssue> issues, CancellationToken ct)
    {
        var configs = await _db.CombinedLessonConfigs
            .Include(c => c.Subject)
            .Include(c => c.Year)
            .Where(c => c.Subject.RequiresSpecialRoom && c.Subject.SpecialRoomId != null)
            .ToListAsync(ct);

        var rooms = await _db.Rooms.ToListAsync(ct);

        foreach (var config in configs)
        {
            var room = rooms.FirstOrDefault(r => r.Id == config.Subject.SpecialRoomId);
            if (room is not null && room.Capacity < config.MaxClassesPerLesson)
            {
                issues.Add(new PreflightIssue(
                    "Error",
                    "RoomCapacity",
                    $"Room '{room.Name}' (capacity {room.Capacity}) cannot fit {config.MaxClassesPerLesson} classes for combined '{config.Subject.Name}' in year '{config.Year.Name}'.",
                    "CombinedLessonConfig",
                    config.Id));
            }
        }
    }
}
