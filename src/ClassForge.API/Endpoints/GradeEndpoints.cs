using ClassForge.API.Filters;
using ClassForge.Application.DTOs.Grades;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class GradeEndpoints
{
    public static RouteGroupBuilder MapGradeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/grades")
            .WithTags("Grades")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll)
            .WithSummary("List all grades")
            .WithDescription("Returns all grades (e.g. Grade 1, Grade 2) for the current tenant, ordered by sort order.")
            .Produces<List<GradeResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a grade by ID")
            .Produces<GradeResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateGradeRequest>>()
            .WithSummary("Create a grade")
            .WithDescription("Creates a new grade level. Grades represent year levels in the school (e.g. Grade 8, Grade 9).")
            .Produces<GradeResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateGradeRequest>>()
            .WithSummary("Update a grade")
            .Produces<GradeResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Delete a grade")
            .WithDescription("Permanently removes a grade and cascades to associated groups and configurations.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(IAppDbContext db)
    {
        var grades = await db.Grades.OrderBy(g => g.SortOrder).ToListAsync();
        return Results.Ok(grades.Select(g => g.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid id, IAppDbContext db)
    {
        var grade = await db.Grades.FindAsync(id);
        return grade is null ? Results.NotFound() : Results.Ok(grade.ToResponse());
    }

    private static async Task<IResult> Create(
        CreateGradeRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId)
            return Results.Unauthorized();

        var grade = request.ToEntity(tenantId);
        db.Grades.Add(grade);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/grades/{grade.Id}", grade.ToResponse());
    }

    private static async Task<IResult> Update(Guid id, UpdateGradeRequest request, IAppDbContext db)
    {
        var grade = await db.Grades.FindAsync(id);
        if (grade is null) return Results.NotFound();

        grade.Name = request.Name;
        grade.SortOrder = request.SortOrder;
        await db.SaveChangesAsync();

        return Results.Ok(grade.ToResponse());
    }

    private static async Task<IResult> Delete(Guid id, IAppDbContext db)
    {
        var grade = await db.Grades.FindAsync(id);
        if (grade is null) return Results.NotFound();

        db.Grades.Remove(grade);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
