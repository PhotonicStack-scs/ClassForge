namespace ClassForge.Application.Interfaces;

public record TimetableGenerationRequest(Guid TimetableId, Guid TenantId);

public interface ITimetableGenerationQueue
{
    ValueTask EnqueueAsync(TimetableGenerationRequest request, CancellationToken cancellationToken = default);
}
