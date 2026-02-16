using ClassForge.API.Filters;
using ClassForge.Application.DTOs.Groups;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class GroupEndpoints
{
    public static RouteGroupBuilder MapGroupEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/grades/{gradeId:guid}/groups")
            .WithTags("Groups")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create).AddEndpointFilter<ValidationFilter<CreateGroupRequest>>();
        group.MapPut("/{id:guid}", Update).AddEndpointFilter<ValidationFilter<UpdateGroupRequest>>();
        group.MapDelete("/{id:guid}", Delete);

        return group;
    }

    private static async Task<IResult> GetAll(Guid gradeId, IAppDbContext db)
    {
        var groups = await db.Groups
            .Where(g => g.GradeId == gradeId)
            .OrderBy(g => g.SortOrder)
            .ToListAsync();
        return Results.Ok(groups.Select(g => g.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid gradeId, Guid id, IAppDbContext db)
    {
        var grp = await db.Groups.FirstOrDefaultAsync(g => g.Id == id && g.GradeId == gradeId);
        return grp is null ? Results.NotFound() : Results.Ok(grp.ToResponse());
    }

    private static async Task<IResult> Create(
        Guid gradeId, CreateGroupRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var gradeExists = await db.Grades.AnyAsync(g => g.Id == gradeId);
        if (!gradeExists) return Results.NotFound(new { error = "Grade not found." });

        var grp = request.ToEntity(tenantId, gradeId);
        db.Groups.Add(grp);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/grades/{gradeId}/groups/{grp.Id}", grp.ToResponse());
    }

    private static async Task<IResult> Update(Guid gradeId, Guid id, UpdateGroupRequest request, IAppDbContext db)
    {
        var grp = await db.Groups.FirstOrDefaultAsync(g => g.Id == id && g.GradeId == gradeId);
        if (grp is null) return Results.NotFound();

        grp.Name = request.Name;
        grp.SortOrder = request.SortOrder;
        await db.SaveChangesAsync();

        return Results.Ok(grp.ToResponse());
    }

    private static async Task<IResult> Delete(Guid gradeId, Guid id, IAppDbContext db)
    {
        var grp = await db.Groups.FirstOrDefaultAsync(g => g.Id == id && g.GradeId == gradeId);
        if (grp is null) return Results.NotFound();

        db.Groups.Remove(grp);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
