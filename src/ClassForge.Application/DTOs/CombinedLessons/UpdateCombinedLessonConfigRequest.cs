namespace ClassForge.Application.DTOs.CombinedLessons;

public record UpdateCombinedLessonConfigRequest(
    bool IsMandatory,
    int MaxGroupsPerLesson,
    List<Guid> GroupIds);
