namespace ClassForge.Application.DTOs.CombinedLessons;

public record CreateCombinedLessonConfigRequest(
    Guid SubjectId,
    bool IsMandatory,
    int MaxClassesPerLesson,
    List<Guid> ClassIds);
