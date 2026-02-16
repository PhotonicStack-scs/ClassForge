using ClassForge.API.Filters;
using ClassForge.Application.DTOs.TeacherSlotBlocks;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class TeacherSlotBlockEndpoints
{
    public static RouteGroupBuilder MapTeacherSlotBlockEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/teachers/{teacherId:guid}/blocked-slots")
            .WithTags("Teacher Blocked Slots")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create).AddEndpointFilter<ValidationFilter<CreateTeacherSlotBlockRequest>>();
        group.MapPut("/{id:guid}", Update);
        group.MapDelete("/{id:guid}", Delete);

        return group;
    }

    private static async Task<IResult> GetAll(Guid teacherId, IAppDbContext db)
    {
        var teacherExists = await db.Teachers.AnyAsync(t => t.Id == teacherId);
        if (!teacherExists) return Results.NotFound(new { error = "Teacher not found." });

        var blocks = await db.TeacherSlotBlocks
            .Where(b => b.TeacherId == teacherId)
            .ToListAsync();
        return Results.Ok(blocks.Select(b => b.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid teacherId, Guid id, IAppDbContext db)
    {
        var entity = await db.TeacherSlotBlocks
            .FirstOrDefaultAsync(b => b.Id == id && b.TeacherId == teacherId);
        return entity is null ? Results.NotFound() : Results.Ok(entity.ToResponse());
    }

    private static async Task<IResult> Create(
        Guid teacherId, CreateTeacherSlotBlockRequest request, IAppDbContext db)
    {
        var teacherExists = await db.Teachers.AnyAsync(t => t.Id == teacherId);
        if (!teacherExists) return Results.NotFound(new { error = "Teacher not found." });

        var entity = request.ToEntity(teacherId);
        db.TeacherSlotBlocks.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/teachers/{teacherId}/blocked-slots/{entity.Id}", entity.ToResponse());
    }

    private static async Task<IResult> Update(
        Guid teacherId, Guid id, UpdateTeacherSlotBlockRequest request, IAppDbContext db)
    {
        var entity = await db.TeacherSlotBlocks
            .FirstOrDefaultAsync(b => b.Id == id && b.TeacherId == teacherId);
        if (entity is null) return Results.NotFound();

        entity.Reason = request.Reason;
        await db.SaveChangesAsync();

        return Results.Ok(entity.ToResponse());
    }

    private static async Task<IResult> Delete(Guid teacherId, Guid id, IAppDbContext db)
    {
        var entity = await db.TeacherSlotBlocks
            .FirstOrDefaultAsync(b => b.Id == id && b.TeacherId == teacherId);
        if (entity is null) return Results.NotFound();

        db.TeacherSlotBlocks.Remove(entity);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
