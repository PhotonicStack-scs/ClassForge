namespace ClassForge.Application.DTOs.Timetables;

public record SchedulingInput(
    List<SchedulingGrade> Grades,
    List<SchedulingGroup> Groups,
    List<SchedulingSubject> Subjects,
    List<SchedulingRoom> Rooms,
    List<SchedulingTeacher> Teachers,
    List<SchedulingTeachingDay> TeachingDays,
    List<SchedulingRequirement> Requirements,
    List<SchedulingCombinedLesson> CombinedLessons,
    List<SchedulingGradeDayConfig> GradeDayConfigs
);

public record SchedulingGrade(Guid Id, string Name, int SortOrder);

public record SchedulingGroup(Guid Id, Guid GradeId, string Name, int SortOrder);

public record SchedulingSubject(
    Guid Id, string Name, bool RequiresSpecialRoom, Guid? SpecialRoomId,
    int MaxPeriodsPerDay, bool AllowDoublePeriods);

public record SchedulingRoom(Guid Id, string Name, int Capacity);

public record SchedulingTeacher(
    Guid Id, string Name,
    List<SchedulingTeacherQualification> Qualifications,
    List<SchedulingTeacherDayConfig> DayConfigs,
    List<Guid> BlockedSlotIds);

public record SchedulingTeacherQualification(Guid SubjectId, int MinGradeSortOrder, int MaxGradeSortOrder);

public record SchedulingTeacherDayConfig(Guid TeachingDayId, int MaxPeriods);

public record SchedulingTeachingDay(Guid Id, int DayOfWeek, int SortOrder, List<SchedulingTimeSlot> TimeSlots);

public record SchedulingTimeSlot(Guid Id, Guid TeachingDayId, int SlotNumber, bool IsBreak);

public record SchedulingRequirement(Guid Id, Guid GradeId, Guid SubjectId, int PeriodsPerWeek, bool PreferDoublePeriods);

public record SchedulingCombinedLesson(
    Guid Id, Guid GradeId, Guid SubjectId, bool IsMandatory,
    int MaxGroupsPerLesson, List<Guid> GroupIds);

public record SchedulingGradeDayConfig(Guid GradeId, Guid TeachingDayId, int MaxPeriods);
