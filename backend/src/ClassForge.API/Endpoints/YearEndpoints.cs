using ClassForge.API.Filters;
using ClassForge.Application.DTOs.Years;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class YearEndpoints
{
    public static RouteGroupBuilder MapYearEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/years")
            .WithTags("Years")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll)
            .WithSummary("List all years")
            .WithDescription("Returns all year levels (e.g. Year 6, Year 7) for the current tenant, ordered by sort order.")
            .Produces<List<YearResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a year by ID")
            .Produces<YearResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateYearRequest>>()
            .WithSummary("Create a year")
            .WithDescription("Creates a new year level. Years represent year levels in the school (e.g. Year 8, Year 9).")
            .Produces<YearResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateYearRequest>>()
            .WithSummary("Update a year")
            .Produces<YearResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Delete a year")
            .WithDescription("Permanently removes a year and cascades to associated classes and configurations.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/bulk", BulkCreate)
            .AddEndpointFilter<ValidationFilter<BulkCreateYearsRequest>>()
            .WithSummary("Bulk create years")
            .Produces<List<YearResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        return group;
    }

    private static async Task<IResult> GetAll(IAppDbContext db)
    {
        var years = await db.Years.OrderBy(y => y.SortOrder).ToListAsync();
        return Results.Ok(years.Select(y => y.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid id, IAppDbContext db)
    {
        var year = await db.Years.FindAsync(id);
        return year is null ? Results.NotFound() : Results.Ok(year.ToResponse());
    }

    private static async Task<IResult> Create(
        CreateYearRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId)
            return Results.Unauthorized();

        var year = request.ToEntity(tenantId);
        db.Years.Add(year);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/years/{year.Id}", year.ToResponse());
    }

    private static async Task<IResult> Update(Guid id, UpdateYearRequest request, IAppDbContext db)
    {
        var year = await db.Years.FindAsync(id);
        if (year is null) return Results.NotFound();

        year.Name = request.Name;
        year.SortOrder = request.SortOrder;
        await db.SaveChangesAsync();

        return Results.Ok(year.ToResponse());
    }

    private static async Task<IResult> Delete(Guid id, IAppDbContext db)
    {
        var year = await db.Years.FindAsync(id);
        if (year is null) return Results.NotFound();

        db.Years.Remove(year);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> BulkCreate(
        BulkCreateYearsRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entities = request.Items.Select(i => i.ToEntity(tenantId)).ToList();
        db.Years.AddRange(entities);
        await db.SaveChangesAsync();

        return Results.Created("/api/v1/years", entities.Select(y => y.ToResponse()).ToList());
    }
}
