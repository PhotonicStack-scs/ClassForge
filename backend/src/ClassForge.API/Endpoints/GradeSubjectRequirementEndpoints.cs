using ClassForge.API.Filters;
using ClassForge.Application.DTOs.GradeSubjectRequirements;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class GradeSubjectRequirementEndpoints
{
    public static RouteGroupBuilder MapGradeSubjectRequirementEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/grades/{gradeId:guid}/subject-requirements")
            .WithTags("Grade Subject Requirements")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll)
            .WithSummary("List subject requirements for a grade")
            .WithDescription("Returns all subject-period requirements for the specified grade. Each defines how many periods per week a subject needs.")
            .Produces<List<GradeSubjectRequirementResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a subject requirement by ID")
            .Produces<GradeSubjectRequirementResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateGradeSubjectRequirementRequest>>()
            .WithSummary("Create a subject requirement")
            .WithDescription("Defines how many periods per week a subject needs for this grade, and whether double periods are preferred.")
            .Produces<GradeSubjectRequirementResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateGradeSubjectRequirementRequest>>()
            .WithSummary("Update a subject requirement")
            .Produces<GradeSubjectRequirementResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Delete a subject requirement")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(Guid gradeId, IAppDbContext db)
    {
        var reqs = await db.GradeSubjectRequirements
            .Where(r => r.GradeId == gradeId)
            .ToListAsync();
        return Results.Ok(reqs.Select(r => r.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid gradeId, Guid id, IAppDbContext db)
    {
        var req = await db.GradeSubjectRequirements
            .FirstOrDefaultAsync(r => r.Id == id && r.GradeId == gradeId);
        return req is null ? Results.NotFound() : Results.Ok(req.ToResponse());
    }

    private static async Task<IResult> Create(
        Guid gradeId, CreateGradeSubjectRequirementRequest request,
        ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entity = request.ToEntity(tenantId, gradeId);
        db.GradeSubjectRequirements.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/grades/{gradeId}/subject-requirements/{entity.Id}", entity.ToResponse());
    }

    private static async Task<IResult> Update(
        Guid gradeId, Guid id, UpdateGradeSubjectRequirementRequest request, IAppDbContext db)
    {
        var entity = await db.GradeSubjectRequirements
            .FirstOrDefaultAsync(r => r.Id == id && r.GradeId == gradeId);
        if (entity is null) return Results.NotFound();

        entity.PeriodsPerWeek = request.PeriodsPerWeek;
        entity.PreferDoublePeriods = request.PreferDoublePeriods;
        await db.SaveChangesAsync();

        return Results.Ok(entity.ToResponse());
    }

    private static async Task<IResult> Delete(Guid gradeId, Guid id, IAppDbContext db)
    {
        var entity = await db.GradeSubjectRequirements
            .FirstOrDefaultAsync(r => r.Id == id && r.GradeId == gradeId);
        if (entity is null) return Results.NotFound();

        db.GradeSubjectRequirements.Remove(entity);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
