using ClassForge.Domain.Entities;

namespace ClassForge.Application.Interfaces;

public interface ITimetableEntryValidator
{
    Task<List<string>> ValidateEntryAsync(Guid timetableId, TimetableEntry entry, CancellationToken cancellationToken = default);
}
