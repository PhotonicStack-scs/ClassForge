namespace ClassForge.Application.Interfaces;

public interface ITenantProvider
{
    Guid? TenantId { get; }
}
