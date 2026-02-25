namespace ClassForge.Application.DTOs.CombinedLessons;

public record CombinedLessonConfigResponse(
    Guid Id,
    Guid YearId,
    Guid SubjectId,
    bool IsMandatory,
    int MaxClassesPerLesson,
    List<Guid> ClassIds);
