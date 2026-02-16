namespace ClassForge.Application.DTOs.CombinedLessons;

public record CombinedLessonConfigResponse(
    Guid Id,
    Guid GradeId,
    Guid SubjectId,
    bool IsMandatory,
    int MaxGroupsPerLesson,
    List<Guid> GroupIds);
