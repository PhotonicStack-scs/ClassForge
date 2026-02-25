using ClassForge.API.Filters;
using ClassForge.Application.DTOs.SchoolDays;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class SchoolDayEndpoints
{
    public static RouteGroupBuilder MapSchoolDayEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/school-days")
            .WithTags("School Days")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll)
            .WithSummary("List all school days")
            .WithDescription("Returns all school days for the current tenant, ordered by sort order. School days represent the weekly schedule (e.g. Monday–Friday).")
            .Produces<List<SchoolDayResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a school day by ID")
            .Produces<SchoolDayResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateSchoolDayRequest>>()
            .WithSummary("Create a school day")
            .WithDescription("Creates a new school day. Each day has a day-of-week, active flag, and sort order.")
            .Produces<SchoolDayResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateSchoolDayRequest>>()
            .WithSummary("Update a school day")
            .WithDescription("Updates the active status and sort order of a school day.")
            .Produces<SchoolDayResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Delete a school day")
            .WithDescription("Permanently removes a school day and its associated time slots.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(IAppDbContext db)
    {
        var days = await db.SchoolDays.OrderBy(d => d.SortOrder).ToListAsync();
        return Results.Ok(days.Select(d => d.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid id, IAppDbContext db)
    {
        var day = await db.SchoolDays.FindAsync(id);
        return day is null ? Results.NotFound() : Results.Ok(day.ToResponse());
    }

    private static async Task<IResult> Create(
        CreateSchoolDayRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entity = request.ToEntity(tenantId);
        db.SchoolDays.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/school-days/{entity.Id}", entity.ToResponse());
    }

    private static async Task<IResult> Update(Guid id, UpdateSchoolDayRequest request, IAppDbContext db)
    {
        var entity = await db.SchoolDays.FindAsync(id);
        if (entity is null) return Results.NotFound();

        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;
        await db.SaveChangesAsync();

        return Results.Ok(entity.ToResponse());
    }

    private static async Task<IResult> Delete(Guid id, IAppDbContext db)
    {
        var entity = await db.SchoolDays.FindAsync(id);
        if (entity is null) return Results.NotFound();

        db.SchoolDays.Remove(entity);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
