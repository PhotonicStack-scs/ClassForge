using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<Year> Years { get; }
    DbSet<Class> Classes { get; }
    DbSet<Subject> Subjects { get; }
    DbSet<Room> Rooms { get; }
    DbSet<YearCurriculum> YearCurricula { get; }
    DbSet<CombinedLessonConfig> CombinedLessonConfigs { get; }
    DbSet<CombinedLessonClass> CombinedLessonClasses { get; }
    DbSet<SchoolDay> SchoolDays { get; }
    DbSet<TimeSlot> TimeSlots { get; }
    DbSet<YearDayConfig> YearDayConfigs { get; }
    DbSet<Teacher> Teachers { get; }
    DbSet<TeacherSubjectQualification> TeacherSubjectQualifications { get; }
    DbSet<TeacherDayConfig> TeacherDayConfigs { get; }
    DbSet<TeacherSlotBlock> TeacherSlotBlocks { get; }
    DbSet<Timetable> Timetables { get; }
    DbSet<TimetableEntry> TimetableEntries { get; }
    DbSet<TimetableEntryClass> TimetableEntryClasses { get; }
    DbSet<TimetableReport> TimetableReports { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
