using ClassForge.Application.DTOs.Timetables;
using ClassForge.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.Infrastructure.Services;

public class SchedulingInputBuilder
{
    private readonly IAppDbContext _db;

    public SchedulingInputBuilder(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<SchedulingInput> BuildAsync(CancellationToken ct = default)
    {
        var grades = await _db.Grades.OrderBy(g => g.SortOrder).ToListAsync(ct);
        var groups = await _db.Groups.OrderBy(g => g.SortOrder).ToListAsync(ct);
        var subjects = await _db.Subjects.ToListAsync(ct);
        var rooms = await _db.Rooms.ToListAsync(ct);

        var teachers = await _db.Teachers
            .Include(t => t.Qualifications).ThenInclude(q => q.MinGrade)
            .Include(t => t.Qualifications).ThenInclude(q => q.MaxGrade)
            .Include(t => t.DayConfigs)
            .Include(t => t.BlockedSlots)
            .ToListAsync(ct);

        var teachingDays = await _db.TeachingDays
            .Include(d => d.TimeSlots.OrderBy(s => s.SlotNumber))
            .Where(d => d.IsActive)
            .OrderBy(d => d.SortOrder)
            .ToListAsync(ct);

        var requirements = await _db.GradeSubjectRequirements.ToListAsync(ct);

        var combinedLessons = await _db.CombinedLessonConfigs
            .Include(c => c.Groups)
            .ToListAsync(ct);

        var gradeDayConfigs = await _db.GradeDayConfigs.ToListAsync(ct);

        return new SchedulingInput(
            grades.Select(g => new SchedulingGrade(g.Id, g.Name, g.SortOrder)).ToList(),
            groups.Select(g => new SchedulingGroup(g.Id, g.GradeId, g.Name, g.SortOrder)).ToList(),
            subjects.Select(s => new SchedulingSubject(s.Id, s.Name, s.RequiresSpecialRoom, s.SpecialRoomId, s.MaxPeriodsPerDay, s.AllowDoublePeriods)).ToList(),
            rooms.Select(r => new SchedulingRoom(r.Id, r.Name, r.Capacity)).ToList(),
            teachers.Select(t => new SchedulingTeacher(
                t.Id, t.Name,
                t.Qualifications.Select(q => new SchedulingTeacherQualification(q.SubjectId, q.MinGrade.SortOrder, q.MaxGrade.SortOrder)).ToList(),
                t.DayConfigs.Select(dc => new SchedulingTeacherDayConfig(dc.TeachingDayId, dc.MaxPeriods)).ToList(),
                t.BlockedSlots.Select(bs => bs.TimeSlotId).ToList()
            )).ToList(),
            teachingDays.Select(d => new SchedulingTeachingDay(
                d.Id, d.DayOfWeek, d.SortOrder,
                d.TimeSlots.Select(s => new SchedulingTimeSlot(s.Id, s.TeachingDayId, s.SlotNumber, s.IsBreak)).ToList()
            )).ToList(),
            requirements.Select(r => new SchedulingRequirement(r.Id, r.GradeId, r.SubjectId, r.PeriodsPerWeek, r.PreferDoublePeriods)).ToList(),
            combinedLessons.Select(c => new SchedulingCombinedLesson(
                c.Id, c.GradeId, c.SubjectId, c.IsMandatory, c.MaxGroupsPerLesson,
                c.Groups.Select(g => g.GroupId).ToList()
            )).ToList(),
            gradeDayConfigs.Select(gc => new SchedulingGradeDayConfig(gc.GradeId, gc.TeachingDayId, gc.MaxPeriods)).ToList()
        );
    }
}
