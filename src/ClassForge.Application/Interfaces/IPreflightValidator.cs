using ClassForge.Application.DTOs.Timetables;

namespace ClassForge.Application.Interfaces;

public interface IPreflightValidator
{
    Task<PreflightResponse> ValidateAsync(CancellationToken cancellationToken = default);
}
