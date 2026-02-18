namespace ClassForge.Application.DTOs.Users;

public record UserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    string? ExternalProvider,
    DateTime CreatedAt);
