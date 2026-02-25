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
        var group = routes.MapGroup("/api/v1/years/{yearId:guid}/combined-lessons")
            .WithTags("Combined Lessons")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll)
            .WithSummary("List combined lesson configs for a year")
            .WithDescription("Returns all combined lesson configurations for the specified year. Combined lessons allow multiple classes to share a time slot for the same subject.")
            .Produces<List<CombinedLessonConfigResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a combined lesson config by ID")
            .Produces<CombinedLessonConfigResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateCombinedLessonConfigRequest>>()
            .WithSummary("Create a combined lesson config")
            .WithDescription("Defines a combined lesson where specified classes share a time slot for a subject. Set isMandatory to true if classes must always be combined.")
            .Produces<CombinedLessonConfigResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateCombinedLessonConfigRequest>>()
            .WithSummary("Update a combined lesson config")
            .WithDescription("Updates the combined lesson settings and replaces the list of participating classes.")
            .Produces<CombinedLessonConfigResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Delete a combined lesson config")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(Guid yearId, IAppDbContext db)
    {
        var configs = await db.CombinedLessonConfigs
            .Include(c => c.Classes)
            .Where(c => c.YearId == yearId)
            .ToListAsync();
        return Results.Ok(configs.Select(c => c.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid yearId, Guid id, IAppDbContext db)
    {
        var config = await db.CombinedLessonConfigs
            .Include(c => c.Classes)
            .FirstOrDefaultAsync(c => c.Id == id && c.YearId == yearId);
        return config is null ? Results.NotFound() : Results.Ok(config.ToResponse());
    }

    private static async Task<IResult> Create(
        Guid yearId, CreateCombinedLessonConfigRequest request,
        ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entity = request.ToEntity(tenantId, yearId);
        db.CombinedLessonConfigs.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/years/{yearId}/combined-lessons/{entity.Id}", entity.ToResponse());
    }

    private static async Task<IResult> Update(
        Guid yearId, Guid id, UpdateCombinedLessonConfigRequest request, IAppDbContext db)
    {
        var entity = await db.CombinedLessonConfigs
            .Include(c => c.Classes)
            .FirstOrDefaultAsync(c => c.Id == id && c.YearId == yearId);
        if (entity is null) return Results.NotFound();

        entity.IsMandatory = request.IsMandatory;
        entity.MaxClassesPerLesson = request.MaxClassesPerLesson;

        // Replace classes
        db.CombinedLessonClasses.RemoveRange(entity.Classes);
        entity.Classes = request.ClassIds
            .Select(cid => new CombinedLessonClass { CombinedLessonConfigId = entity.Id, ClassId = cid })
            .ToList();

        await db.SaveChangesAsync();

        return Results.Ok(entity.ToResponse());
    }

    private static async Task<IResult> Delete(Guid yearId, Guid id, IAppDbContext db)
    {
        var entity = await db.CombinedLessonConfigs
            .FirstOrDefaultAsync(c => c.Id == id && c.YearId == yearId);
        if (entity is null) return Results.NotFound();

        db.CombinedLessonConfigs.Remove(entity);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
