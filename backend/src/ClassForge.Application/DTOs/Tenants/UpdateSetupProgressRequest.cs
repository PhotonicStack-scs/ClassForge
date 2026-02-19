namespace ClassForge.Application.DTOs.Tenants;

public record UpdateSetupProgressRequest(bool SetupCompleted, Dictionary<string, bool>? SetupProgress);
