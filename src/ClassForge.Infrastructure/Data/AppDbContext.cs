using System.Linq.Expressions;
using ClassForge.Application.Interfaces;
using ClassForge.Domain.Entities;
using ClassForge.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    private readonly ITenantProvider _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<GradeSubjectRequirement> GradeSubjectRequirements => Set<GradeSubjectRequirement>();
    public DbSet<CombinedLessonConfig> CombinedLessonConfigs => Set<CombinedLessonConfig>();
    public DbSet<CombinedLessonGroup> CombinedLessonGroups => Set<CombinedLessonGroup>();
    public DbSet<TeachingDay> TeachingDays => Set<TeachingDay>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<GradeDayConfig> GradeDayConfigs => Set<GradeDayConfig>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<TeacherSubjectQualification> TeacherSubjectQualifications => Set<TeacherSubjectQualification>();
    public DbSet<TeacherDayConfig> TeacherDayConfigs => Set<TeacherDayConfig>();
    public DbSet<TeacherSlotBlock> TeacherSlotBlocks => Set<TeacherSlotBlock>();
    public DbSet<Timetable> Timetables => Set<Timetable>();
    public DbSet<TimetableEntry> TimetableEntries => Set<TimetableEntry>();
    public DbSet<TimetableEntryGroup> TimetableEntryGroups => Set<TimetableEntryGroup>();
    public DbSet<TimetableReport> TimetableReports => Set<TimetableReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Apply global query filter for all ITenantEntity types
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var tenantIdProperty = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
            var tenantProvider = Expression.Constant(_tenantProvider);
            var currentTenantId = Expression.Property(tenantProvider, nameof(ITenantProvider.TenantId));

            // e.TenantId == _tenantProvider.TenantId
            // We need to handle nullable Guid from ITenantProvider
            var tenantIdAsNullable = Expression.Convert(tenantIdProperty, typeof(Guid?));
            var filter = Expression.Equal(tenantIdAsNullable, currentTenantId);
            var lambda = Expression.Lambda(filter, parameter);

            entityType.SetQueryFilter(lambda);
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
