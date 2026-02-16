using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<Grade> Grades { get; }
    DbSet<Group> Groups { get; }
    DbSet<Subject> Subjects { get; }
    DbSet<Room> Rooms { get; }
    DbSet<GradeSubjectRequirement> GradeSubjectRequirements { get; }
    DbSet<CombinedLessonConfig> CombinedLessonConfigs { get; }
    DbSet<CombinedLessonGroup> CombinedLessonGroups { get; }
    DbSet<TeachingDay> TeachingDays { get; }
    DbSet<TimeSlot> TimeSlots { get; }
    DbSet<GradeDayConfig> GradeDayConfigs { get; }
    DbSet<Teacher> Teachers { get; }
    DbSet<TeacherSubjectQualification> TeacherSubjectQualifications { get; }
    DbSet<TeacherDayConfig> TeacherDayConfigs { get; }
    DbSet<TeacherSlotBlock> TeacherSlotBlocks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
