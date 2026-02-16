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

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create).AddEndpointFilter<ValidationFilter<CreateGradeRequest>>();
        group.MapPut("/{id:guid}", Update).AddEndpointFilter<ValidationFilter<UpdateGradeRequest>>();
        group.MapDelete("/{id:guid}", Delete);

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
