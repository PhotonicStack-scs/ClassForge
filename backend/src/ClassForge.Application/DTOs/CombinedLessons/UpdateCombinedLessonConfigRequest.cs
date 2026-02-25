namespace ClassForge.Application.DTOs.CombinedLessons;

public record UpdateCombinedLessonConfigRequest(
    bool IsMandatory,
    int MaxClassesPerLesson,
    List<Guid> ClassIds);
