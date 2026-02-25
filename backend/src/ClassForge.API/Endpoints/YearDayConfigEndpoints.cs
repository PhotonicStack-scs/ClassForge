using ClassForge.API.Filters;
using ClassForge.Application.DTOs.YearDayConfigs;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class YearDayConfigEndpoints
{
    public static RouteGroupBuilder MapYearDayConfigEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/years/{yearId:guid}/day-config")
            .WithTags("Year Day Config")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll)
            .WithSummary("List day configs for a year")
            .WithDescription("Returns all day-specific period limits for the specified year. Each config caps how many periods a year can have on a given school day.")
            .Produces<List<YearDayConfigResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a year day config by ID")
            .Produces<YearDayConfigResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateYearDayConfigRequest>>()
            .WithSummary("Create a year day config")
            .WithDescription("Sets the maximum number of teaching periods a year can have on a specific school day.")
            .Produces<YearDayConfigResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateYearDayConfigRequest>>()
            .WithSummary("Update a year day config")
            .Produces<YearDayConfigResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Delete a year day config")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(Guid yearId, IAppDbContext db)
    {
        var configs = await db.YearDayConfigs
            .Where(c => c.YearId == yearId)
            .ToListAsync();
        return Results.Ok(configs.Select(c => c.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid yearId, Guid id, IAppDbContext db)
    {
        var config = await db.YearDayConfigs
            .FirstOrDefaultAsync(c => c.Id == id && c.YearId == yearId);
        return config is null ? Results.NotFound() : Results.Ok(config.ToResponse());
    }

    private static async Task<IResult> Create(
        Guid yearId, CreateYearDayConfigRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entity = request.ToEntity(tenantId, yearId);
        db.YearDayConfigs.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/years/{yearId}/day-config/{entity.Id}", entity.ToResponse());
    }

    private static async Task<IResult> Update(Guid yearId, Guid id, UpdateYearDayConfigRequest request, IAppDbContext db)
    {
        var entity = await db.YearDayConfigs
            .FirstOrDefaultAsync(c => c.Id == id && c.YearId == yearId);
        if (entity is null) return Results.NotFound();

        entity.MaxPeriods = request.MaxPeriods;
        await db.SaveChangesAsync();

        return Results.Ok(entity.ToResponse());
    }

    private static async Task<IResult> Delete(Guid yearId, Guid id, IAppDbContext db)
    {
        var entity = await db.YearDayConfigs
            .FirstOrDefaultAsync(c => c.Id == id && c.YearId == yearId);
        if (entity is null) return Results.NotFound();

        db.YearDayConfigs.Remove(entity);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
