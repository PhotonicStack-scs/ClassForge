namespace ClassForge.Domain.Entities;

public class TimetableEntryClass
{
    public Guid TimetableEntryId { get; set; }
    public Guid ClassId { get; set; }

    public TimetableEntry TimetableEntry { get; set; } = null!;
    public Class Class { get; set; } = null!;
}
