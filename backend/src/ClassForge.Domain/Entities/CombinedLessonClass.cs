namespace ClassForge.Domain.Entities;

public class CombinedLessonClass
{
    public Guid CombinedLessonConfigId { get; set; }
    public Guid ClassId { get; set; }

    public CombinedLessonConfig CombinedLessonConfig { get; set; } = null!;
    public Class Class { get; set; } = null!;
}
