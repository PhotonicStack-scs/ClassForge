using ClassForge.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ClassForge.Infrastructure.Services;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid? _overrideTenantId;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            if (_overrideTenantId.HasValue)
                return _overrideTenantId;

            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("tenant_id");
            return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    public void SetTenantId(Guid tenantId)
    {
        _overrideTenantId = tenantId;
    }
}
