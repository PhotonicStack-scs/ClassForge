using ClassForge.Application.DTOs.Auth;
using ClassForge.Application.DTOs.CombinedLessons;
using ClassForge.Application.DTOs.GradeDayConfigs;
using ClassForge.Application.DTOs.Grades;
using ClassForge.Application.DTOs.GradeSubjectRequirements;
using ClassForge.Application.DTOs.Groups;
using ClassForge.Application.DTOs.Rooms;
using ClassForge.Application.DTOs.Subjects;
using ClassForge.Application.DTOs.TeacherDayConfigs;
using ClassForge.Application.DTOs.TeacherQualifications;
using ClassForge.Application.DTOs.Teachers;
using ClassForge.Application.DTOs.TeacherSlotBlocks;
using ClassForge.Application.DTOs.TeachingDays;
using ClassForge.Application.DTOs.Tenants;
using ClassForge.Application.DTOs.Timetables;
using ClassForge.Application.DTOs.TimeSlots;
using ClassForge.Application.DTOs.Users;
using ClassForge.Domain.Entities;
using ClassForge.Domain.Enums;

namespace ClassForge.Application.Mapping;

public static class MappingExtensions
{
    // Tenant
    public static TenantResponse ToResponse(this Tenant tenant) =>
        new(tenant.Id, tenant.Name, tenant.CreatedAt);

    // User
    public static UserResponse ToResponse(this User user) =>
        new(user.Id, user.Email, user.DisplayName, user.Role.ToString(), user.ExternalProvider, user.CreatedAt);

    public static UserProfileResponse ToProfileResponse(this User user) =>
        new(user.Id, user.TenantId, user.Email, user.DisplayName, user.Role.ToString(), user.ExternalProvider);

    // Grade
    public static GradeResponse ToResponse(this Grade grade) =>
        new(grade.Id, grade.Name, grade.SortOrder);

    public static Grade ToEntity(this CreateGradeRequest request, Guid tenantId) =>
        new() { Id = Guid.NewGuid(), TenantId = tenantId, Name = request.Name, SortOrder = request.SortOrder };

    // Group
    public static GroupResponse ToResponse(this Group group) =>
        new(group.Id, group.GradeId, group.Name, group.SortOrder);

    public static Group ToEntity(this CreateGroupRequest request, Guid tenantId, Guid gradeId) =>
        new() { Id = Guid.NewGuid(), TenantId = tenantId, GradeId = gradeId, Name = request.Name, SortOrder = request.SortOrder };

    // Subject
    public static SubjectResponse ToResponse(this Subject subject) =>
        new(subject.Id, subject.Name, subject.RequiresSpecialRoom, subject.SpecialRoomId, subject.MaxPeriodsPerDay, subject.AllowDoublePeriods);

    public static Subject ToEntity(this CreateSubjectRequest request, Guid tenantId) =>
        new()
        {
            Id = Guid.NewGuid(), TenantId = tenantId, Name = request.Name,
            RequiresSpecialRoom = request.RequiresSpecialRoom, SpecialRoomId = request.SpecialRoomId,
            MaxPeriodsPerDay = request.MaxPeriodsPerDay, AllowDoublePeriods = request.AllowDoublePeriods
        };

    // Room
    public static RoomResponse ToResponse(this Room room) =>
        new(room.Id, room.Name, room.Capacity);

    public static Room ToEntity(this CreateRoomRequest request, Guid tenantId) =>
        new() { Id = Guid.NewGuid(), TenantId = tenantId, Name = request.Name, Capacity = request.Capacity };

    // GradeSubjectRequirement
    public static GradeSubjectRequirementResponse ToResponse(this GradeSubjectRequirement r) =>
        new(r.Id, r.GradeId, r.SubjectId, r.PeriodsPerWeek, r.PreferDoublePeriods);

    public static GradeSubjectRequirement ToEntity(this CreateGradeSubjectRequirementRequest request, Guid tenantId, Guid gradeId) =>
        new()
        {
            Id = Guid.NewGuid(), TenantId = tenantId, GradeId = gradeId,
            SubjectId = request.SubjectId, PeriodsPerWeek = request.PeriodsPerWeek,
            PreferDoublePeriods = request.PreferDoublePeriods
        };

    // CombinedLessonConfig
    public static CombinedLessonConfigResponse ToResponse(this CombinedLessonConfig config) =>
        new(config.Id, config.GradeId, config.SubjectId, config.IsMandatory, config.MaxGroupsPerLesson,
            config.Groups.Select(g => g.GroupId).ToList());

    public static CombinedLessonConfig ToEntity(this CreateCombinedLessonConfigRequest request, Guid tenantId, Guid gradeId) =>
        new()
        {
            Id = Guid.NewGuid(), TenantId = tenantId, GradeId = gradeId,
            SubjectId = request.SubjectId, IsMandatory = request.IsMandatory,
            MaxGroupsPerLesson = request.MaxGroupsPerLesson,
            Groups = request.GroupIds.Select(gid => new CombinedLessonGroup { GroupId = gid }).ToList()
        };

    // TeachingDay
    public static TeachingDayResponse ToResponse(this TeachingDay day) =>
        new(day.Id, day.DayOfWeek, day.IsActive, day.SortOrder);

    public static TeachingDay ToEntity(this CreateTeachingDayRequest request, Guid tenantId) =>
        new() { Id = Guid.NewGuid(), TenantId = tenantId, DayOfWeek = request.DayOfWeek, IsActive = request.IsActive, SortOrder = request.SortOrder };

    // TimeSlot
    public static TimeSlotResponse ToResponse(this TimeSlot slot) =>
        new(slot.Id, slot.TeachingDayId, slot.SlotNumber, slot.StartTime.ToString("HH:mm"), slot.EndTime.ToString("HH:mm"), slot.IsBreak);

    public static TimeSlot ToEntity(this CreateTimeSlotRequest request, Guid tenantId, Guid teachingDayId) =>
        new()
        {
            Id = Guid.NewGuid(), TenantId = tenantId, TeachingDayId = teachingDayId,
            SlotNumber = request.SlotNumber, StartTime = TimeOnly.Parse(request.StartTime),
            EndTime = TimeOnly.Parse(request.EndTime), IsBreak = request.IsBreak
        };

    // GradeDayConfig
    public static GradeDayConfigResponse ToResponse(this GradeDayConfig config) =>
        new(config.Id, config.GradeId, config.TeachingDayId, config.MaxPeriods);

    public static GradeDayConfig ToEntity(this CreateGradeDayConfigRequest request, Guid tenantId, Guid gradeId) =>
        new()
        {
            Id = Guid.NewGuid(), TenantId = tenantId, GradeId = gradeId,
            TeachingDayId = request.TeachingDayId, MaxPeriods = request.MaxPeriods
        };

    // Teacher
    public static TeacherResponse ToResponse(this Teacher teacher) =>
        new(teacher.Id, teacher.Name, teacher.Email);

    public static Teacher ToEntity(this CreateTeacherRequest request, Guid tenantId) =>
        new() { Id = Guid.NewGuid(), TenantId = tenantId, Name = request.Name, Email = request.Email };

    // TeacherSubjectQualification
    public static TeacherQualificationResponse ToResponse(this TeacherSubjectQualification q) =>
        new(q.Id, q.SubjectId, q.MinGradeId, q.MaxGradeId);

    public static TeacherSubjectQualification ToEntity(this CreateTeacherQualificationRequest request, Guid teacherId) =>
        new()
        {
            Id = Guid.NewGuid(), TeacherId = teacherId,
            SubjectId = request.SubjectId, MinGradeId = request.MinGradeId, MaxGradeId = request.MaxGradeId
        };

    // TeacherDayConfig
    public static TeacherDayConfigResponse ToResponse(this TeacherDayConfig config) =>
        new(config.Id, config.TeachingDayId, config.MaxPeriods);

    public static TeacherDayConfig ToEntity(this CreateTeacherDayConfigRequest request, Guid teacherId) =>
        new()
        {
            Id = Guid.NewGuid(), TeacherId = teacherId,
            TeachingDayId = request.TeachingDayId, MaxPeriods = request.MaxPeriods
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
    public static TimetableResponse ToResponse(this Timetable timetable) =>
        new(timetable.Id, timetable.Name, timetable.Status.ToString(), timetable.GeneratedAt,
            timetable.QualityScore, timetable.CreatedBy, timetable.ErrorMessage, timetable.CreatedAt);

    // TimetableEntry
    public static TimetableEntryResponse ToResponse(this TimetableEntry entry) =>
        new(entry.Id, entry.TimeSlotId, entry.SubjectId, entry.TeacherId,
            entry.RoomId, entry.IsDoublePeriod, entry.CombinedLessonGroupId,
            entry.Groups.Select(g => g.GroupId).ToList());

    // TimetableReport
    public static TimetableReportResponse ToResponse(this TimetableReport report) =>
        new(report.Id, report.Type.ToString(), report.Category, report.Message,
            report.RelatedEntityType, report.RelatedEntityId);
}
