using ClassForge.API.Filters;
using ClassForge.Application.DTOs.Curricula;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class YearCurriculumEndpoints
{
    public static RouteGroupBuilder MapYearCurriculumEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/years/{yearId:guid}/curriculum")
            .WithTags("Curriculum")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll)
            .WithSummary("List curriculum for a year")
            .WithDescription("Returns all subject-period requirements for the specified year. Each defines how many periods per week a subject needs.")
            .Produces<List<YearCurriculumResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a curriculum entry by ID")
            .Produces<YearCurriculumResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateYearCurriculumRequest>>()
            .WithSummary("Create a curriculum entry")
            .WithDescription("Defines how many periods per week a subject needs for this year, and whether double periods are preferred.")
            .Produces<YearCurriculumResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPost("/bulk", BulkCreate)
            .AddEndpointFilter<ValidationFilter<BulkCreateYearCurriculaRequest>>()
            .WithSummary("Bulk create curriculum entries")
            .Produces<List<YearCurriculumResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateYearCurriculumRequest>>()
            .WithSummary("Update a curriculum entry")
            .Produces<YearCurriculumResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Delete a curriculum entry")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(Guid yearId, IAppDbContext db)
    {
        var items = await db.YearCurricula
            .Include(r => r.Subject)
            .Where(r => r.YearId == yearId)
            .ToListAsync();
        return Results.Ok(items.Select(r => r.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid yearId, Guid id, IAppDbContext db)
    {
        var item = await db.YearCurricula
            .Include(r => r.Subject)
            .FirstOrDefaultAsync(r => r.Id == id && r.YearId == yearId);
        return item is null ? Results.NotFound() : Results.Ok(item.ToResponse());
    }

    private static async Task<IResult> Create(
        Guid yearId, CreateYearCurriculumRequest request,
        ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entity = request.ToEntity(tenantId, yearId);
        db.YearCurricula.Add(entity);
        await db.SaveChangesAsync();

        var loaded = await db.YearCurricula
            .Include(r => r.Subject)
            .FirstAsync(r => r.Id == entity.Id);

        return Results.Created($"/api/v1/years/{yearId}/curriculum/{entity.Id}", loaded.ToResponse());
    }

    private static async Task<IResult> BulkCreate(
        Guid yearId, BulkCreateYearCurriculaRequest request,
        ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entities = request.Items.Select(i => i.ToEntity(tenantId, yearId)).ToList();
        db.YearCurricula.AddRange(entities);
        await db.SaveChangesAsync();

        var ids = entities.Select(e => e.Id).ToList();
        var loaded = await db.YearCurricula
            .Include(r => r.Subject)
            .Where(r => ids.Contains(r.Id))
            .ToListAsync();

        return Results.Created($"/api/v1/years/{yearId}/curriculum", loaded.Select(r => r.ToResponse()).ToList());
    }

    private static async Task<IResult> Update(
        Guid yearId, Guid id, UpdateYearCurriculumRequest request, IAppDbContext db)
    {
        var entity = await db.YearCurricula
            .Include(r => r.Subject)
            .FirstOrDefaultAsync(r => r.Id == id && r.YearId == yearId);
        if (entity is null) return Results.NotFound();

        entity.PeriodsPerWeek = request.PeriodsPerWeek;
        entity.PreferDoublePeriods = request.PreferDoublePeriods;
        entity.MaxPeriodsPerDay = request.MaxPeriodsPerDay;
        entity.AllowDoublePeriods = request.AllowDoublePeriods;
        await db.SaveChangesAsync();

        return Results.Ok(entity.ToResponse());
    }

    private static async Task<IResult> Delete(Guid yearId, Guid id, IAppDbContext db)
    {
        var entity = await db.YearCurricula
            .FirstOrDefaultAsync(r => r.Id == id && r.YearId == yearId);
        if (entity is null) return Results.NotFound();

        db.YearCurricula.Remove(entity);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
