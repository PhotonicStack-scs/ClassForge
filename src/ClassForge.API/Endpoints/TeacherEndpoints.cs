using ClassForge.API.Filters;
using ClassForge.Application.DTOs.Teachers;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class TeacherEndpoints
{
    public static RouteGroupBuilder MapTeacherEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/teachers")
            .WithTags("Teachers")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll)
            .WithSummary("List all teachers")
            .WithDescription("Returns all teachers for the current tenant, ordered by name.")
            .Produces<List<TeacherResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a teacher by ID")
            .Produces<TeacherResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateTeacherRequest>>()
            .WithSummary("Create a teacher")
            .WithDescription("Creates a new teacher. Email is optional and can be used to link the teacher to a Viewer user account.")
            .Produces<TeacherResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateTeacherRequest>>()
            .WithSummary("Update a teacher")
            .Produces<TeacherResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Delete a teacher")
            .WithDescription("Permanently removes a teacher and cascades to qualifications, day configs, and blocked slots.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(IAppDbContext db)
    {
        var teachers = await db.Teachers.OrderBy(t => t.Name).ToListAsync();
        return Results.Ok(teachers.Select(t => t.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid id, IAppDbContext db)
    {
        var teacher = await db.Teachers.FindAsync(id);
        return teacher is null ? Results.NotFound() : Results.Ok(teacher.ToResponse());
    }

    private static async Task<IResult> Create(
        CreateTeacherRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entity = request.ToEntity(tenantId);
        db.Teachers.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/teachers/{entity.Id}", entity.ToResponse());
    }

    private static async Task<IResult> Update(Guid id, UpdateTeacherRequest request, IAppDbContext db)
    {
        var entity = await db.Teachers.FindAsync(id);
        if (entity is null) return Results.NotFound();

        entity.Name = request.Name;
        entity.Email = request.Email;
        await db.SaveChangesAsync();

        return Results.Ok(entity.ToResponse());
    }

    private static async Task<IResult> Delete(Guid id, IAppDbContext db)
    {
        var entity = await db.Teachers.FindAsync(id);
        if (entity is null) return Results.NotFound();

        db.Teachers.Remove(entity);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
