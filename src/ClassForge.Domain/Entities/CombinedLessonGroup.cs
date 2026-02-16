namespace ClassForge.Domain.Entities;

public class CombinedLessonGroup
{
    public Guid CombinedLessonConfigId { get; set; }
    public Guid GroupId { get; set; }

    public CombinedLessonConfig CombinedLessonConfig { get; set; } = null!;
    public Group Group { get; set; } = null!;
}
