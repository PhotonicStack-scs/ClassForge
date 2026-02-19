using System.Text.Json;
using ClassForge.API.Filters;
using ClassForge.Application.DTOs.School;
using ClassForge.Application.DTOs.Tenants;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using ClassForge.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class TenantEndpoints
{
    public static RouteGroupBuilder MapTenantEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/school")
            .WithTags("School")
            .RequireAuthorization();

        group.MapGet("/", GetSchool)
            .WithSummary("Get school details")
            .WithDescription("Returns the current tenant's school details.")
            .Produces<TenantResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/", UpdateSchool)
            .AddEndpointFilter<ValidationFilter<UpdateTenantRequest>>()
            .RequireAuthorization("OrgAdmin")
            .WithSummary("Update school details")
            .WithDescription("Updates the school name and default language. Requires OrgAdmin role.")
            .Produces<TenantResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPut("/setup-progress", UpdateSetupProgress)
            .AddEndpointFilter<ValidationFilter<UpdateSetupProgressRequest>>()
            .RequireAuthorization("OrgAdmin")
            .WithSummary("Update setup progress")
            .WithDescription("Updates the school's setup completion status and per-step progress. Requires OrgAdmin role.")
            .Produces<TenantResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapGet("/stats", GetStats)
            .WithSummary("Get dashboard statistics")
            .WithDescription("Returns counts of all major entities and the published timetable ID if available.")
            .Produces<DashboardStatsResponse>();

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
        if (request.DefaultLanguage is not null) tenant.DefaultLanguage = request.DefaultLanguage;
        await db.SaveChangesAsync();

        return Results.Ok(tenant.ToResponse());
    }

    private static async Task<IResult> UpdateSetupProgress(
        UpdateSetupProgressRequest request,
        ITenantProvider tenantProvider,
        IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId)
            return Results.Unauthorized();

        var tenant = await db.Tenants.FindAsync(tenantId);
        if (tenant is null)
            return Results.NotFound();

        tenant.SetupCompleted = request.SetupCompleted;
        tenant.SetupProgressJson = request.SetupProgress is not null
            ? JsonSerializer.Serialize(request.SetupProgress)
            : null;
        await db.SaveChangesAsync();

        return Results.Ok(tenant.ToResponse());
    }

    private static async Task<IResult> GetStats(ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { })
            return Results.Unauthorized();

        var publishedId = await db.Timetables
            .Where(t => t.Status == TimetableStatus.Published)
            .OrderByDescending(t => t.UpdatedAt)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync();

        return Results.Ok(new DashboardStatsResponse(
            await db.Grades.CountAsync(),
            await db.Groups.CountAsync(),
            await db.Teachers.CountAsync(),
            await db.Subjects.CountAsync(),
            await db.Rooms.CountAsync(),
            await db.Timetables.CountAsync(),
            publishedId));
    }
}
