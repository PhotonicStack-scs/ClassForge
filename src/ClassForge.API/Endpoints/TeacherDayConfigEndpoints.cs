using ClassForge.API.Filters;
using ClassForge.Application.DTOs.TeacherDayConfigs;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class TeacherDayConfigEndpoints
{
    public static RouteGroupBuilder MapTeacherDayConfigEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/teachers/{teacherId:guid}/day-config")
            .WithTags("Teacher Day Config")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create).AddEndpointFilter<ValidationFilter<CreateTeacherDayConfigRequest>>();
        group.MapPut("/{id:guid}", Update).AddEndpointFilter<ValidationFilter<UpdateTeacherDayConfigRequest>>();
        group.MapDelete("/{id:guid}", Delete);

        return group;
    }

    private static async Task<IResult> GetAll(Guid teacherId, IAppDbContext db)
    {
        var teacherExists = await db.Teachers.AnyAsync(t => t.Id == teacherId);
        if (!teacherExists) return Results.NotFound(new { error = "Teacher not found." });

        var configs = await db.TeacherDayConfigs
            .Where(c => c.TeacherId == teacherId)
            .ToListAsync();
        return Results.Ok(configs.Select(c => c.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid teacherId, Guid id, IAppDbContext db)
    {
        var entity = await db.TeacherDayConfigs
            .FirstOrDefaultAsync(c => c.Id == id && c.TeacherId == teacherId);
        return entity is null ? Results.NotFound() : Results.Ok(entity.ToResponse());
    }

    private static async Task<IResult> Create(
        Guid teacherId, CreateTeacherDayConfigRequest request, IAppDbContext db)
    {
        var teacherExists = await db.Teachers.AnyAsync(t => t.Id == teacherId);
        if (!teacherExists) return Results.NotFound(new { error = "Teacher not found." });

        var entity = request.ToEntity(teacherId);
        db.TeacherDayConfigs.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/teachers/{teacherId}/day-config/{entity.Id}", entity.ToResponse());
    }

    private static async Task<IResult> Update(
        Guid teacherId, Guid id, UpdateTeacherDayConfigRequest request, IAppDbContext db)
    {
        var entity = await db.TeacherDayConfigs
            .FirstOrDefaultAsync(c => c.Id == id && c.TeacherId == teacherId);
        if (entity is null) return Results.NotFound();

        entity.MaxPeriods = request.MaxPeriods;
        await db.SaveChangesAsync();

        return Results.Ok(entity.ToResponse());
    }

    private static async Task<IResult> Delete(Guid teacherId, Guid id, IAppDbContext db)
    {
        var entity = await db.TeacherDayConfigs
            .FirstOrDefaultAsync(c => c.Id == id && c.TeacherId == teacherId);
        if (entity is null) return Results.NotFound();

        db.TeacherDayConfigs.Remove(entity);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
