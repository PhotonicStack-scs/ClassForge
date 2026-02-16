namespace ClassForge.Application.DTOs.CombinedLessons;

public record CreateCombinedLessonConfigRequest(
    Guid SubjectId,
    bool IsMandatory,
    int MaxGroupsPerLesson,
    List<Guid> GroupIds);
