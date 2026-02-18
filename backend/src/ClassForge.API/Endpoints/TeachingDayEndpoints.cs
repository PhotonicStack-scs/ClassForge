using ClassForge.API.Filters;
using ClassForge.Application.DTOs.TeachingDays;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class TeachingDayEndpoints
{
    public static RouteGroupBuilder MapTeachingDayEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/teaching-days")
            .WithTags("Teaching Days")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll)
            .WithSummary("List all teaching days")
            .WithDescription("Returns all teaching days for the current tenant, ordered by sort order. Teaching days represent the weekly schedule (e.g. Monday–Friday).")
            .Produces<List<TeachingDayResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a teaching day by ID")
            .Produces<TeachingDayResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateTeachingDayRequest>>()
            .WithSummary("Create a teaching day")
            .WithDescription("Creates a new teaching day. Each day has a day-of-week, active flag, and sort order.")
            .Produces<TeachingDayResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateTeachingDayRequest>>()
            .WithSummary("Update a teaching day")
            .WithDescription("Updates the active status and sort order of a teaching day.")
            .Produces<TeachingDayResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Delete a teaching day")
            .WithDescription("Permanently removes a teaching day and its associated time slots.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(IAppDbContext db)
    {
        var days = await db.TeachingDays.OrderBy(d => d.SortOrder).ToListAsync();
        return Results.Ok(days.Select(d => d.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid id, IAppDbContext db)
    {
        var day = await db.TeachingDays.FindAsync(id);
        return day is null ? Results.NotFound() : Results.Ok(day.ToResponse());
    }

    private static async Task<IResult> Create(
        CreateTeachingDayRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entity = request.ToEntity(tenantId);
        db.TeachingDays.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/teaching-days/{entity.Id}", entity.ToResponse());
    }

    private static async Task<IResult> Update(Guid id, UpdateTeachingDayRequest request, IAppDbContext db)
    {
        var entity = await db.TeachingDays.FindAsync(id);
        if (entity is null) return Results.NotFound();

        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;
        await db.SaveChangesAsync();

        return Results.Ok(entity.ToResponse());
    }

    private static async Task<IResult> Delete(Guid id, IAppDbContext db)
    {
        var entity = await db.TeachingDays.FindAsync(id);
        if (entity is null) return Results.NotFound();

        db.TeachingDays.Remove(entity);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
