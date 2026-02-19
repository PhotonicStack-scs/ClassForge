namespace ClassForge.Application.DTOs.Tenants;

public record TenantResponse(
    Guid Id,
    string Name,
    string DefaultLanguage,
    bool SetupCompleted,
    Dictionary<string, bool>? SetupProgress,
    DateTime CreatedAt);
