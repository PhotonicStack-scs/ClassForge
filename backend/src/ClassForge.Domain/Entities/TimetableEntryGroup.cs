namespace ClassForge.Domain.Entities;

public class TimetableEntryGroup
{
    public Guid TimetableEntryId { get; set; }
    public Guid GroupId { get; set; }

    public TimetableEntry TimetableEntry { get; set; } = null!;
    public Group Group { get; set; } = null!;
}
