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

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create).AddEndpointFilter<ValidationFilter<CreateTeacherRequest>>();
        group.MapPut("/{id:guid}", Update).AddEndpointFilter<ValidationFilter<UpdateTeacherRequest>>();
        group.MapDelete("/{id:guid}", Delete);

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
