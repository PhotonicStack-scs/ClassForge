using ClassForge.API.Filters;
using ClassForge.Application.DTOs.GradeDayConfigs;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class GradeDayConfigEndpoints
{
    public static RouteGroupBuilder MapGradeDayConfigEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/grades/{gradeId:guid}/day-config")
            .WithTags("Grade Day Config")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll)
            .WithSummary("List day configs for a grade")
            .WithDescription("Returns all day-specific period limits for the specified grade. Each config caps how many periods a grade can have on a given teaching day.")
            .Produces<List<GradeDayConfigResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a grade day config by ID")
            .Produces<GradeDayConfigResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateGradeDayConfigRequest>>()
            .WithSummary("Create a grade day config")
            .WithDescription("Sets the maximum number of teaching periods a grade can have on a specific teaching day.")
            .Produces<GradeDayConfigResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateGradeDayConfigRequest>>()
            .WithSummary("Update a grade day config")
            .Produces<GradeDayConfigResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Delete a grade day config")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(Guid gradeId, IAppDbContext db)
    {
        var configs = await db.GradeDayConfigs
            .Where(c => c.GradeId == gradeId)
            .ToListAsync();
        return Results.Ok(configs.Select(c => c.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid gradeId, Guid id, IAppDbContext db)
    {
        var config = await db.GradeDayConfigs
            .FirstOrDefaultAsync(c => c.Id == id && c.GradeId == gradeId);
        return config is null ? Results.NotFound() : Results.Ok(config.ToResponse());
    }

    private static async Task<IResult> Create(
        Guid gradeId, CreateGradeDayConfigRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entity = request.ToEntity(tenantId, gradeId);
        db.GradeDayConfigs.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/grades/{gradeId}/day-config/{entity.Id}", entity.ToResponse());
    }

    private static async Task<IResult> Update(Guid gradeId, Guid id, UpdateGradeDayConfigRequest request, IAppDbContext db)
    {
        var entity = await db.GradeDayConfigs
            .FirstOrDefaultAsync(c => c.Id == id && c.GradeId == gradeId);
        if (entity is null) return Results.NotFound();

        entity.MaxPeriods = request.MaxPeriods;
        await db.SaveChangesAsync();

        return Results.Ok(entity.ToResponse());
    }

    private static async Task<IResult> Delete(Guid gradeId, Guid id, IAppDbContext db)
    {
        var entity = await db.GradeDayConfigs
            .FirstOrDefaultAsync(c => c.Id == id && c.GradeId == gradeId);
        if (entity is null) return Results.NotFound();

        db.GradeDayConfigs.Remove(entity);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
