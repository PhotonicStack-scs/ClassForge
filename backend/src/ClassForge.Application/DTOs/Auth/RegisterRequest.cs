namespace ClassForge.Application.DTOs.Auth;

public record RegisterRequest(
    string SchoolName,
    string Email,
    string Password,
    string DisplayName);
