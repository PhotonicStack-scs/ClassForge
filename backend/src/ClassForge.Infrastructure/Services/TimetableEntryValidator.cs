using ClassForge.Application.Interfaces;
using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.Infrastructure.Services;

public class TimetableEntryValidator : ITimetableEntryValidator
{
    private readonly IAppDbContext _db;

    public TimetableEntryValidator(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<string>> ValidateEntryAsync(
        Guid timetableId, TimetableEntry entry, CancellationToken cancellationToken = default)
    {
        var issues = new List<string>();
        var classIds = entry.Classes.Select(c => c.ClassId).ToList();

        var otherEntries = await _db.TimetableEntries
            .Include(e => e.Classes)
            .Where(e => e.TimetableId == timetableId && e.Id != entry.Id)
            .ToListAsync(cancellationToken);

        var timeSlot = await _db.TimeSlots
            .Include(s => s.SchoolDay)
            .FirstOrDefaultAsync(s => s.Id == entry.TimeSlotId, cancellationToken);

        if (timeSlot is null)
        {
            issues.Add($"Entry {entry.Id}: Time slot not found.");
            return issues;
        }

        // HC-1: No teacher double-booking
        var teacherConflicts = otherEntries.Where(e =>
            e.TeacherId == entry.TeacherId && e.TimeSlotId == entry.TimeSlotId);
        foreach (var conflict in teacherConflicts)
            issues.Add($"Entry {entry.Id}: Teacher double-booked at slot {entry.TimeSlotId} (conflicts with entry {conflict.Id}).");

        // HC-2: No class double-booking
        foreach (var classId in classIds)
        {
            var classConflicts = otherEntries.Where(e =>
                e.TimeSlotId == entry.TimeSlotId && e.Classes.Any(c => c.ClassId == classId));
            foreach (var conflict in classConflicts)
                issues.Add($"Entry {entry.Id}: Class {classId} double-booked at slot {entry.TimeSlotId} (conflicts with entry {conflict.Id}).");
        }

        // HC-3: Room conflict
        if (entry.RoomId is not null)
        {
            var roomConflicts = otherEntries.Where(e =>
                e.RoomId == entry.RoomId && e.TimeSlotId == entry.TimeSlotId);
            foreach (var conflict in roomConflicts)
                issues.Add($"Entry {entry.Id}: Room {entry.RoomId} double-booked at slot {entry.TimeSlotId} (conflicts with entry {conflict.Id}).");
        }

        // HC-4: Teacher blocked slots
        var teacher = await _db.Teachers
            .Include(t => t.BlockedSlots)
            .FirstOrDefaultAsync(t => t.Id == entry.TeacherId, cancellationToken);
        if (teacher is not null && teacher.BlockedSlots.Any(bs => bs.TimeSlotId == entry.TimeSlotId))
            issues.Add($"Entry {entry.Id}: Teacher '{teacher.Name}' is blocked at this time slot.");

        // HC-5: Teacher daily limit
        if (teacher is not null)
        {
            var teacherDayConfig = await _db.TeacherDayConfigs
                .FirstOrDefaultAsync(dc => dc.TeacherId == teacher.Id && dc.SchoolDayId == timeSlot.SchoolDayId, cancellationToken);

            if (teacherDayConfig is not null)
            {
                var teacherDayCount = otherEntries.Count(e =>
                {
                    var otherSlot = _db.TimeSlots.FirstOrDefault(s => s.Id == e.TimeSlotId);
                    return e.TeacherId == entry.TeacherId && otherSlot?.SchoolDayId == timeSlot.SchoolDayId;
                }) + 1;

                if (teacherDayCount > teacherDayConfig.MaxPeriods)
                    issues.Add($"Entry {entry.Id}: Teacher '{teacher.Name}' exceeds daily limit of {teacherDayConfig.MaxPeriods} periods.");
            }
        }

        // HC-7: Special room requirement
        var subject = await _db.Subjects.FindAsync([entry.SubjectId], cancellationToken);
        if (subject is not null && subject.RequiresSpecialRoom && entry.RoomId != subject.SpecialRoomId)
            issues.Add($"Entry {entry.Id}: Subject '{subject.Name}' requires special room {subject.SpecialRoomId} but assigned to {entry.RoomId}.");

        // HC-8: Year day config limit
        foreach (var classId in classIds)
        {
            var schoolClass = await _db.Classes.FindAsync([classId], cancellationToken);
            if (schoolClass is null) continue;

            var yearDayConfig = await _db.YearDayConfigs
                .FirstOrDefaultAsync(c => c.YearId == schoolClass.YearId && c.SchoolDayId == timeSlot.SchoolDayId, cancellationToken);

            if (yearDayConfig is not null)
            {
                var nonBreakPosition = await _db.TimeSlots
                    .CountAsync(s => s.SchoolDayId == timeSlot.SchoolDayId && !s.IsBreak && s.SlotNumber <= timeSlot.SlotNumber, cancellationToken);
                if (nonBreakPosition > yearDayConfig.MaxPeriods)
                    issues.Add($"Entry {entry.Id}: Slot exceeds year day config limit of {yearDayConfig.MaxPeriods} for class '{schoolClass.Name}'.");
            }
        }

        // HC-9: Double period check
        if (entry.IsDoublePeriod)
        {
            var nextSlot = await _db.TimeSlots
                .Where(s => s.SchoolDayId == timeSlot.SchoolDayId && !s.IsBreak && s.SlotNumber > timeSlot.SlotNumber)
                .OrderBy(s => s.SlotNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextSlot is null)
                issues.Add($"Entry {entry.Id}: Double period requires a consecutive non-break slot but none exists.");
        }

        // HC-10: Subject daily limit (per year curriculum)
        foreach (var classId in classIds)
        {
            var schoolClass = await _db.Classes.FindAsync([classId], cancellationToken);
            if (schoolClass is null) continue;

            var curriculum = await _db.YearCurricula
                .FirstOrDefaultAsync(r => r.YearId == schoolClass.YearId && r.SubjectId == entry.SubjectId, cancellationToken);
            if (curriculum is null) continue;

            var subjectDayCount = otherEntries.Count(e =>
            {
                if (e.SubjectId != entry.SubjectId) return false;
                if (!e.Classes.Any(c => c.ClassId == classId)) return false;
                var otherSlot = _db.TimeSlots.FirstOrDefault(s => s.Id == e.TimeSlotId);
                return otherSlot?.SchoolDayId == timeSlot.SchoolDayId;
            }) + 1;

            if (subjectDayCount > curriculum.MaxPeriodsPerDay)
                issues.Add($"Entry {entry.Id}: Subject '{subject?.Name}' exceeds daily limit of {curriculum.MaxPeriodsPerDay} for class {classId}.");
        }

        return issues;
    }
}
