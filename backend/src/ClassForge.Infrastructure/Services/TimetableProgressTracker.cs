using System.Collections.Concurrent;

namespace ClassForge.Infrastructure.Services;

public class TimetableProgressTracker
{
    private readonly ConcurrentDictionary<Guid, int> _progress = new();

    public void Set(Guid id, int pct) => _progress[id] = pct;
    public int? Get(Guid id) => _progress.TryGetValue(id, out var p) ? p : null;
    public void Remove(Guid id) => _progress.TryRemove(id, out _);
}
