namespace ClassForge.Application.DTOs.Auth;

public record UserProfileResponse(
    Guid Id,
    Guid TenantId,
    string Email,
    string DisplayName,
    string Role,
    string? ExternalProvider);
