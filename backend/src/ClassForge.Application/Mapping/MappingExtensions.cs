using System.Text.Json;
using ClassForge.Application.DTOs.Auth;
using ClassForge.Application.DTOs.Classes;
using ClassForge.Application.DTOs.CombinedLessons;
using ClassForge.Application.DTOs.Curricula;
using ClassForge.Application.DTOs.Rooms;
using ClassForge.Application.DTOs.SchoolDays;
using ClassForge.Application.DTOs.Subjects;
using ClassForge.Application.DTOs.TeacherDayConfigs;
using ClassForge.Application.DTOs.TeacherQualifications;
using ClassForge.Application.DTOs.Teachers;
using ClassForge.Application.DTOs.TeacherSlotBlocks;
using ClassForge.Application.DTOs.Tenants;
using ClassForge.Application.DTOs.Timetables;
using ClassForge.Application.DTOs.TimeSlots;
using ClassForge.Application.DTOs.Users;
using ClassForge.Application.DTOs.YearDayConfigs;
using ClassForge.Application.DTOs.Years;
using ClassForge.Domain.Entities;
using ClassForge.Domain.Enums;

namespace ClassForge.Application.Mapping;

public static class MappingExtensions
{
    private static readonly string[] SubjectColorPalette =
    [
        "#DBEAFE", "#FEE2E2", "#D1FAE5", "#FEF3C7", "#EDE9FE",
        "#FCE7F3", "#E0F2FE", "#F0FDF4", "#FFF7ED", "#F5F3FF",
        "#ECFDF5", "#FEF9C3", "#F0F9FF", "#FDF4FF", "#FFEDD5",
        "#E0F7FA", "#F3E8FF", "#FFF1F2", "#ECFEFF", "#F7FEE7"
    ];

    // Tenant
    public static TenantResponse ToResponse(this Tenant tenant) =>
        new(tenant.Id, tenant.Name, tenant.DefaultLanguage, tenant.SetupCompleted,
            tenant.SetupProgressJson is not null
                ? JsonSerializer.Deserialize<Dictionary<string, bool>>(tenant.SetupProgressJson)
                : null,
            tenant.CreatedAt);

    // User
    public static UserResponse ToResponse(this User user) =>
        new(user.Id, user.Email, user.DisplayName, user.Role.ToString(), user.ExternalProvider,
            user.LanguagePreference, user.CreatedAt);

    public static UserProfileResponse ToProfileResponse(this User user) =>
        new(user.Id, user.TenantId, user.Email, user.DisplayName, user.Role.ToString(),
            user.ExternalProvider, user.LanguagePreference);

    // Year
    public static YearResponse ToResponse(this Year year) =>
        new(year.Id, year.Name, year.SortOrder);

    public static Year ToEntity(this CreateYearRequest request, Guid tenantId) =>
        new() { Id = Guid.NewGuid(), TenantId = tenantId, Name = request.Name, SortOrder = request.SortOrder };

    // Class
    public static ClassResponse ToResponse(this Class @class) =>
        new(@class.Id, @class.YearId, @class.Name, @class.SortOrder);

    public static Class ToEntity(this CreateClassRequest request, Guid tenantId, Guid yearId) =>
        new() { Id = Guid.NewGuid(), TenantId = tenantId, YearId = yearId, Name = request.Name, SortOrder = request.SortOrder };

    // Subject
    public static SubjectResponse ToResponse(this Subject subject) =>
        new(subject.Id, subject.Name, subject.RequiresSpecialRoom, subject.SpecialRoomId, subject.Color);

    public static Subject ToEntity(this CreateSubjectRequest request, Guid tenantId) =>
        new()
        {
            Id = Guid.NewGuid(), TenantId = tenantId, Name = request.Name,
            RequiresSpecialRoom = request.RequiresSpecialRoom, SpecialRoomId = request.SpecialRoomId,
            Color = request.Color ?? SubjectColorPalette[Random.Shared.Next(SubjectColorPalette.Length)]
        };

    // Room
    public static RoomResponse ToResponse(this Room room) =>
        new(room.Id, room.Name, room.Capacity);

    public static Room ToEntity(this CreateRoomRequest request, Guid tenantId) =>
        new() { Id = Guid.NewGuid(), TenantId = tenantId, Name = request.Name, Capacity = request.Capacity };

    // YearCurriculum
    public static YearCurriculumResponse ToResponse(this YearCurriculum r) =>
        new(r.Id, r.YearId, r.SubjectId, r.PeriodsPerWeek, r.PreferDoublePeriods,
            r.MaxPeriodsPerDay, r.AllowDoublePeriods, r.Subject.Name);

    public static YearCurriculum ToEntity(this CreateYearCurriculumRequest request, Guid tenantId, Guid yearId) =>
        new()
        {
            Id = Guid.NewGuid(), TenantId = tenantId, YearId = yearId,
            SubjectId = request.SubjectId, PeriodsPerWeek = request.PeriodsPerWeek,
            PreferDoublePeriods = request.PreferDoublePeriods,
            MaxPeriodsPerDay = request.MaxPeriodsPerDay, AllowDoublePeriods = request.AllowDoublePeriods
        };

    // CombinedLessonConfig
    public static CombinedLessonConfigResponse ToResponse(this CombinedLessonConfig config) =>
        new(config.Id, config.YearId, config.SubjectId, config.IsMandatory, config.MaxClassesPerLesson,
            config.Classes.Select(c => c.ClassId).ToList());

    public static CombinedLessonConfig ToEntity(this CreateCombinedLessonConfigRequest request, Guid tenantId, Guid yearId) =>
        new()
        {
            Id = Guid.NewGuid(), TenantId = tenantId, YearId = yearId,
            SubjectId = request.SubjectId, IsMandatory = request.IsMandatory,
            MaxClassesPerLesson = request.MaxClassesPerLesson,
            Classes = request.ClassIds.Select(cid => new CombinedLessonClass { ClassId = cid }).ToList()
        };

    // SchoolDay
    public static SchoolDayResponse ToResponse(this SchoolDay day) =>
        new(day.Id, day.DayOfWeek, ((DayOfWeek)day.DayOfWeek).ToString(), day.IsActive, day.SortOrder);

    public static SchoolDay ToEntity(this CreateSchoolDayRequest request, Guid tenantId) =>
        new() { Id = Guid.NewGuid(), TenantId = tenantId, DayOfWeek = request.DayOfWeek, IsActive = request.IsActive, SortOrder = request.SortOrder };

    // TimeSlot
    public static TimeSlotResponse ToResponse(this TimeSlot slot) =>
        new(slot.Id, slot.SchoolDayId, slot.SlotNumber, slot.StartTime.ToString("HH:mm"), slot.EndTime.ToString("HH:mm"), slot.IsBreak);

    public static TimeSlot ToEntity(this CreateTimeSlotRequest request, Guid tenantId, Guid schoolDayId) =>
        new()
        {
            Id = Guid.NewGuid(), TenantId = tenantId, SchoolDayId = schoolDayId,
            SlotNumber = request.SlotNumber, StartTime = TimeOnly.Parse(request.StartTime),
            EndTime = TimeOnly.Parse(request.EndTime), IsBreak = request.IsBreak
        };

    // YearDayConfig
    public static YearDayConfigResponse ToResponse(this YearDayConfig config) =>
        new(config.Id, config.YearId, config.SchoolDayId, config.MaxPeriods);

    public static YearDayConfig ToEntity(this CreateYearDayConfigRequest request, Guid tenantId, Guid yearId) =>
        new()
        {
            Id = Guid.NewGuid(), TenantId = tenantId, YearId = yearId,
            SchoolDayId = request.SchoolDayId, MaxPeriods = request.MaxPeriods
        };

    // Teacher
    public static TeacherResponse ToResponse(this Teacher teacher) =>
        new(teacher.Id, teacher.Name, teacher.Email);

    public static Teacher ToEntity(this CreateTeacherRequest request, Guid tenantId) =>
        new() { Id = Guid.NewGuid(), TenantId = tenantId, Name = request.Name, Email = request.Email };

    // TeacherSubjectQualification
    public static TeacherQualificationResponse ToResponse(this TeacherSubjectQualification q) =>
        new(q.Id, q.SubjectId, q.MinYearId, q.MaxYearId, q.Subject.Name, q.MinYear.Name, q.MaxYear.Name);

    public static TeacherSubjectQualification ToEntity(this CreateTeacherQualificationRequest request, Guid teacherId) =>
        new()
        {
            Id = Guid.NewGuid(), TeacherId = teacherId,
            SubjectId = request.SubjectId, MinYearId = request.MinYearId, MaxYearId = request.MaxYearId
        };

    // TeacherDayConfig
    public static TeacherDayConfigResponse ToResponse(this TeacherDayConfig config) =>
        new(config.Id, config.SchoolDayId, config.MaxPeriods);

    public static TeacherDayConfig ToEntity(this CreateTeacherDayConfigRequest request, Guid teacherId) =>
        new()
        {
            Id = Guid.NewGuid(), TeacherId = teacherId,
            SchoolDayId = request.SchoolDayId, MaxPeriods = request.MaxPeriods
        };

    // TeacherSlotBlock
    public static TeacherSlotBlockResponse ToResponse(this TeacherSlotBlock block) =>
        new(block.Id, block.TimeSlotId, block.Reason);

    public static TeacherSlotBlock ToEntity(this CreateTeacherSlotBlockRequest request, Guid teacherId) =>
        new()
        {
            Id = Guid.NewGuid(), TeacherId = teacherId,
            TimeSlotId = request.TimeSlotId, Reason = request.Reason
        };

    // Timetable
    public static TimetableResponse ToResponse(this Timetable timetable, int? progressPercentage = null) =>
        new(timetable.Id, timetable.Name, timetable.Status.ToString(), timetable.GeneratedAt,
            timetable.QualityScore, timetable.CreatedBy, timetable.ErrorMessage, timetable.CreatedAt,
            progressPercentage);

    // TimetableEntry
    public static TimetableEntryResponse ToResponse(this TimetableEntry entry) =>
        new(entry.Id, entry.TimeSlotId, entry.SubjectId, entry.TeacherId,
            entry.RoomId, entry.IsDoublePeriod, entry.CombinedLessonClassId,
            entry.Classes.Select(c => c.ClassId).ToList());

    // TimetableReport
    public static TimetableReportResponse ToResponse(this TimetableReport report) =>
        new(report.Id, report.Type.ToString(), report.Category, report.Message,
            report.RelatedEntityType, report.RelatedEntityId);
}
