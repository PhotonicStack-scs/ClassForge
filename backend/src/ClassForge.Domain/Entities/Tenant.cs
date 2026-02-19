using ClassForge.Domain.Interfaces;

namespace ClassForge.Domain.Entities;

public class Tenant : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DefaultLanguage { get; set; } = "nb";
    public bool SetupCompleted { get; set; } = false;
    public string? SetupProgressJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
