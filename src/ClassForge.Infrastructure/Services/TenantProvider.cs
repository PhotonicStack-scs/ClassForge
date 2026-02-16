using System.Security.Claims;
using ClassForge.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ClassForge.Infrastructure.Services;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("tenant_id");
            return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }
    }
}
