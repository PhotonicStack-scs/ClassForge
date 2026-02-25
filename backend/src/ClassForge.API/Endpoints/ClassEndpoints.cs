using ClassForge.API.Filters;
using ClassForge.Application.DTOs.Classes;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class ClassEndpoints
{
    public static RouteGroupBuilder MapClassEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/years/{yearId:guid}/classes")
            .WithTags("Classes")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll)
            .WithSummary("List classes for a year")
            .WithDescription("Returns all classes within the specified year, ordered by sort order.")
            .Produces<List<ClassResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a class by ID")
            .Produces<ClassResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateClassRequest>>()
            .WithSummary("Create a class")
            .WithDescription("Creates a new class within a year. Classes represent class divisions (e.g. 8A, 8B).")
            .Produces<ClassResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateClassRequest>>()
            .WithSummary("Update a class")
            .Produces<ClassResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Delete a class")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(Guid yearId, IAppDbContext db)
    {
        var classes = await db.Classes
            .Where(c => c.YearId == yearId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
        return Results.Ok(classes.Select(c => c.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid yearId, Guid id, IAppDbContext db)
    {
        var @class = await db.Classes.FirstOrDefaultAsync(c => c.Id == id && c.YearId == yearId);
        return @class is null ? Results.NotFound() : Results.Ok(@class.ToResponse());
    }

    private static async Task<IResult> Create(
        Guid yearId, CreateClassRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var yearExists = await db.Years.AnyAsync(y => y.Id == yearId);
        if (!yearExists) return Results.NotFound(new { error = "Year not found." });

        var @class = request.ToEntity(tenantId, yearId);
        db.Classes.Add(@class);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/years/{yearId}/classes/{@class.Id}", @class.ToResponse());
    }

    private static async Task<IResult> Update(Guid yearId, Guid id, UpdateClassRequest request, IAppDbContext db)
    {
        var @class = await db.Classes.FirstOrDefaultAsync(c => c.Id == id && c.YearId == yearId);
        if (@class is null) return Results.NotFound();

        @class.Name = request.Name;
        @class.SortOrder = request.SortOrder;
        await db.SaveChangesAsync();

        return Results.Ok(@class.ToResponse());
    }

    private static async Task<IResult> Delete(Guid yearId, Guid id, IAppDbContext db)
    {
        var @class = await db.Classes.FirstOrDefaultAsync(c => c.Id == id && c.YearId == yearId);
        if (@class is null) return Results.NotFound();

        db.Classes.Remove(@class);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
