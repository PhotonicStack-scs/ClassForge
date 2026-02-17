using ClassForge.Application.DTOs.Timetables;

namespace ClassForge.Application.Interfaces;

public interface ITimetableGenerator
{
    Task<TimetableGenerationResult> GenerateAsync(
        SchedulingInput input,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);
}
