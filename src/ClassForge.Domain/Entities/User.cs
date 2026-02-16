using ClassForge.Domain.Enums;
using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class User : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? ExternalProvider { get; set; }
    public string? ExternalId { get; set; }
    public UserRole Role { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
