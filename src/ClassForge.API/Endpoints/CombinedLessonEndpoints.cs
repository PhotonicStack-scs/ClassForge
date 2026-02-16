using ClassForge.API.Filters;
using ClassForge.Application.DTOs.CombinedLessons;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class CombinedLessonEndpoints
{
    public static RouteGroupBuilder MapCombinedLessonEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/grades/{gradeId:guid}/combined-lessons")
            .WithTags("Combined Lessons")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create).AddEndpointFilter<ValidationFilter<CreateCombinedLessonConfigRequest>>();
        group.MapPut("/{id:guid}", Update).AddEndpointFilter<ValidationFilter<UpdateCombinedLessonConfigRequest>>();
        group.MapDelete("/{id:guid}", Delete);

        return group;
    }

    private static async Task<IResult> GetAll(Guid gradeId, IAppDbContext db)
    {
        var configs = await db.CombinedLessonConfigs
            .Include(c => c.Groups)
            .Where(c => c.GradeId == gradeId)
            .ToListAsync();
        return Results.Ok(configs.Select(c => c.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid gradeId, Guid id, IAppDbContext db)
    {
        var config = await db.CombinedLessonConfigs
            .Include(c => c.Groups)
            .FirstOrDefaultAsync(c => c.Id == id && c.GradeId == gradeId);
        return config is null ? Results.NotFound() : Results.Ok(config.ToResponse());
    }

    private static async Task<IResult> Create(
        Guid gradeId, CreateCombinedLessonConfigRequest request,
        ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entity = request.ToEntity(tenantId, gradeId);
        db.CombinedLessonConfigs.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/grades/{gradeId}/combined-lessons/{entity.Id}", entity.ToResponse());
    }

    private static async Task<IResult> Update(
        Guid gradeId, Guid id, UpdateCombinedLessonConfigRequest request, IAppDbContext db)
    {
        var entity = await db.CombinedLessonConfigs
            .Include(c => c.Groups)
            .FirstOrDefaultAsync(c => c.Id == id && c.GradeId == gradeId);
        if (entity is null) return Results.NotFound();

        entity.IsMandatory = request.IsMandatory;
        entity.MaxGroupsPerLesson = request.MaxGroupsPerLesson;

        // Replace groups
        db.CombinedLessonGroups.RemoveRange(entity.Groups);
        entity.Groups = request.GroupIds
            .Select(gid => new CombinedLessonGroup { CombinedLessonConfigId = entity.Id, GroupId = gid })
            .ToList();

        await db.SaveChangesAsync();

        return Results.Ok(entity.ToResponse());
    }

    private static async Task<IResult> Delete(Guid gradeId, Guid id, IAppDbContext db)
    {
        var entity = await db.CombinedLessonConfigs
            .FirstOrDefaultAsync(c => c.Id == id && c.GradeId == gradeId);
        if (entity is null) return Results.NotFound();

        db.CombinedLessonConfigs.Remove(entity);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
