namespace ClassForge.Application.DTOs.Users;

public record CreateUserRequest(string Email, string Password, string DisplayName, string Role);
