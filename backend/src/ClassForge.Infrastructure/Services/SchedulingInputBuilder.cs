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
        var years = await _db.Years.OrderBy(g => g.SortOrder).ToListAsync(ct);
        var classes = await _db.Classes.OrderBy(g => g.SortOrder).ToListAsync(ct);
        var subjects = await _db.Subjects.ToListAsync(ct);
        var rooms = await _db.Rooms.ToListAsync(ct);

        var teachers = await _db.Teachers
            .Include(t => t.Qualifications).ThenInclude(q => q.MinYear)
            .Include(t => t.Qualifications).ThenInclude(q => q.MaxYear)
            .Include(t => t.DayConfigs)
            .Include(t => t.BlockedSlots)
            .ToListAsync(ct);

        var schoolDays = await _db.SchoolDays
            .Include(d => d.TimeSlots.OrderBy(s => s.SlotNumber))
            .Where(d => d.IsActive)
            .OrderBy(d => d.SortOrder)
            .ToListAsync(ct);

        var requirements = await _db.YearCurricula.ToListAsync(ct);

        var combinedLessons = await _db.CombinedLessonConfigs
            .Include(c => c.Classes)
            .ToListAsync(ct);

        var yearDayConfigs = await _db.YearDayConfigs.ToListAsync(ct);

        return new SchedulingInput(
            years.Select(g => new SchedulingYear(g.Id, g.Name, g.SortOrder)).ToList(),
            classes.Select(g => new SchedulingClass(g.Id, g.YearId, g.Name, g.SortOrder)).ToList(),
            subjects.Select(s => new SchedulingSubject(s.Id, s.Name, s.RequiresSpecialRoom, s.SpecialRoomId)).ToList(),
            rooms.Select(r => new SchedulingRoom(r.Id, r.Name, r.Capacity)).ToList(),
            teachers.Select(t => new SchedulingTeacher(
                t.Id, t.Name,
                t.Qualifications.Select(q => new SchedulingTeacherQualification(q.SubjectId, q.MinYear.SortOrder, q.MaxYear.SortOrder)).ToList(),
                t.DayConfigs.Select(dc => new SchedulingTeacherDayConfig(dc.SchoolDayId, dc.MaxPeriods)).ToList(),
                t.BlockedSlots.Select(bs => bs.TimeSlotId).ToList()
            )).ToList(),
            schoolDays.Select(d => new SchedulingSchoolDay(
                d.Id, d.DayOfWeek, d.SortOrder,
                d.TimeSlots.Select(s => new SchedulingTimeSlot(s.Id, s.SchoolDayId, s.SlotNumber, s.IsBreak)).ToList()
            )).ToList(),
            requirements.Select(r => new SchedulingRequirement(r.Id, r.YearId, r.SubjectId, r.PeriodsPerWeek, r.PreferDoublePeriods, r.MaxPeriodsPerDay, r.AllowDoublePeriods)).ToList(),
            combinedLessons.Select(c => new SchedulingCombinedLesson(
                c.Id, c.YearId, c.SubjectId, c.IsMandatory, c.MaxClassesPerLesson,
                c.Classes.Select(cl => cl.ClassId).ToList()
            )).ToList(),
            yearDayConfigs.Select(gc => new SchedulingYearDayConfig(gc.YearId, gc.SchoolDayId, gc.MaxPeriods)).ToList()
        );
    }
}
