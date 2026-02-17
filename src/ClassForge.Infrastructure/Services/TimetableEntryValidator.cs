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
        var groupIds = entry.Groups.Select(g => g.GroupId).ToList();

        var otherEntries = await _db.TimetableEntries
            .Include(e => e.Groups)
            .Where(e => e.TimetableId == timetableId && e.Id != entry.Id)
            .ToListAsync(cancellationToken);

        var timeSlot = await _db.TimeSlots
            .Include(s => s.TeachingDay)
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

        // HC-2: No group double-booking
        foreach (var groupId in groupIds)
        {
            var groupConflicts = otherEntries.Where(e =>
                e.TimeSlotId == entry.TimeSlotId && e.Groups.Any(g => g.GroupId == groupId));
            foreach (var conflict in groupConflicts)
                issues.Add($"Entry {entry.Id}: Group {groupId} double-booked at slot {entry.TimeSlotId} (conflicts with entry {conflict.Id}).");
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
                .FirstOrDefaultAsync(dc => dc.TeacherId == teacher.Id && dc.TeachingDayId == timeSlot.TeachingDayId, cancellationToken);

            if (teacherDayConfig is not null)
            {
                var teacherDayCount = otherEntries.Count(e =>
                {
                    var otherSlot = _db.TimeSlots.FirstOrDefault(s => s.Id == e.TimeSlotId);
                    return e.TeacherId == entry.TeacherId && otherSlot?.TeachingDayId == timeSlot.TeachingDayId;
                }) + 1;

                if (teacherDayCount > teacherDayConfig.MaxPeriods)
                    issues.Add($"Entry {entry.Id}: Teacher '{teacher.Name}' exceeds daily limit of {teacherDayConfig.MaxPeriods} periods.");
            }
        }

        // HC-7: Special room requirement
        var subject = await _db.Subjects.FindAsync([entry.SubjectId], cancellationToken);
        if (subject is not null && subject.RequiresSpecialRoom && entry.RoomId != subject.SpecialRoomId)
            issues.Add($"Entry {entry.Id}: Subject '{subject.Name}' requires special room {subject.SpecialRoomId} but assigned to {entry.RoomId}.");

        // HC-8: Grade day config limit
        foreach (var groupId in groupIds)
        {
            var group = await _db.Groups.FindAsync([groupId], cancellationToken);
            if (group is null) continue;

            var gradeDayConfig = await _db.GradeDayConfigs
                .FirstOrDefaultAsync(c => c.GradeId == group.GradeId && c.TeachingDayId == timeSlot.TeachingDayId, cancellationToken);

            if (gradeDayConfig is not null)
            {
                var nonBreakPosition = await _db.TimeSlots
                    .CountAsync(s => s.TeachingDayId == timeSlot.TeachingDayId && !s.IsBreak && s.SlotNumber <= timeSlot.SlotNumber, cancellationToken);
                if (nonBreakPosition > gradeDayConfig.MaxPeriods)
                    issues.Add($"Entry {entry.Id}: Slot exceeds grade day config limit of {gradeDayConfig.MaxPeriods} for group '{group.Name}'.");
            }
        }

        // HC-9: Double period check
        if (entry.IsDoublePeriod)
        {
            var nextSlot = await _db.TimeSlots
                .Where(s => s.TeachingDayId == timeSlot.TeachingDayId && !s.IsBreak && s.SlotNumber > timeSlot.SlotNumber)
                .OrderBy(s => s.SlotNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextSlot is null)
                issues.Add($"Entry {entry.Id}: Double period requires a consecutive non-break slot but none exists.");
        }

        // HC-10: Subject daily limit
        if (subject is not null)
        {
            foreach (var groupId in groupIds)
            {
                var subjectDayCount = otherEntries.Count(e =>
                {
                    if (e.SubjectId != entry.SubjectId) return false;
                    if (!e.Groups.Any(g => g.GroupId == groupId)) return false;
                    var otherSlot = _db.TimeSlots.FirstOrDefault(s => s.Id == e.TimeSlotId);
                    return otherSlot?.TeachingDayId == timeSlot.TeachingDayId;
                }) + 1;

                if (subjectDayCount > subject.MaxPeriodsPerDay)
                    issues.Add($"Entry {entry.Id}: Subject '{subject.Name}' exceeds daily limit of {subject.MaxPeriodsPerDay} for group {groupId}.");
            }
        }

        return issues;
    }
}
