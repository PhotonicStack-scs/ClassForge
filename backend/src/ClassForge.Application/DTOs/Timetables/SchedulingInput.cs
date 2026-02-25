namespace ClassForge.Application.DTOs.Timetables;

public record SchedulingInput(
    List<SchedulingYear> Years,
    List<SchedulingClass> Classes,
    List<SchedulingSubject> Subjects,
    List<SchedulingRoom> Rooms,
    List<SchedulingTeacher> Teachers,
    List<SchedulingSchoolDay> SchoolDays,
    List<SchedulingRequirement> Requirements,
    List<SchedulingCombinedLesson> CombinedLessons,
    List<SchedulingYearDayConfig> YearDayConfigs
);

public record SchedulingYear(Guid Id, string Name, int SortOrder);

public record SchedulingClass(Guid Id, Guid YearId, string Name, int SortOrder);

public record SchedulingSubject(
    Guid Id, string Name, bool RequiresSpecialRoom, Guid? SpecialRoomId);

public record SchedulingRoom(Guid Id, string Name, int Capacity);

public record SchedulingTeacher(
    Guid Id, string Name,
    List<SchedulingTeacherQualification> Qualifications,
    List<SchedulingTeacherDayConfig> DayConfigs,
    List<Guid> BlockedSlotIds);

public record SchedulingTeacherQualification(Guid SubjectId, int MinYearSortOrder, int MaxYearSortOrder);

public record SchedulingTeacherDayConfig(Guid SchoolDayId, int MaxPeriods);

public record SchedulingSchoolDay(Guid Id, int DayOfWeek, int SortOrder, List<SchedulingTimeSlot> TimeSlots);

public record SchedulingTimeSlot(Guid Id, Guid SchoolDayId, int SlotNumber, bool IsBreak);

public record SchedulingRequirement(Guid Id, Guid YearId, Guid SubjectId, int PeriodsPerWeek, bool PreferDoublePeriods, int MaxPeriodsPerDay, bool AllowDoublePeriods);

public record SchedulingCombinedLesson(
    Guid Id, Guid YearId, Guid SubjectId, bool IsMandatory,
    int MaxClassesPerLesson, List<Guid> ClassIds);

public record SchedulingYearDayConfig(Guid YearId, Guid SchoolDayId, int MaxPeriods);
