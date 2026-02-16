using ClassForge.API.Filters;
using ClassForge.Application.DTOs.Tenants;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class TenantEndpoints
{
    public static RouteGroupBuilder MapTenantEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/school")
            .WithTags("School")
            .RequireAuthorization();

        group.MapGet("/", GetSchool);
        group.MapPut("/", UpdateSchool)
            .AddEndpointFilter<ValidationFilter<UpdateTenantRequest>>()
            .RequireAuthorization("OrgAdmin");

        return group;
    }

    private static async Task<IResult> GetSchool(ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId)
            return Results.Unauthorized();

        var tenant = await db.Tenants.FindAsync(tenantId);
        return tenant is null ? Results.NotFound() : Results.Ok(tenant.ToResponse());
    }

    private static async Task<IResult> UpdateSchool(
        UpdateTenantRequest request,
        ITenantProvider tenantProvider,
        IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId)
            return Results.Unauthorized();

        var tenant = await db.Tenants.FindAsync(tenantId);
        if (tenant is null)
            return Results.NotFound();

        tenant.Name = request.Name;
        await db.SaveChangesAsync();

        return Results.Ok(tenant.ToResponse());
    }
}
