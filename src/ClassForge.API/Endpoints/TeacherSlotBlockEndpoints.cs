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

        group.MapGet("/", GetAll)
            .WithSummary("List blocked slots for a teacher")
            .WithDescription("Returns all time slots that are blocked for the specified teacher. Blocked slots prevent the scheduler from assigning lessons during those times.")
            .Produces<List<TeacherSlotBlockResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a blocked slot by ID")
            .Produces<TeacherSlotBlockResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateTeacherSlotBlockRequest>>()
            .WithSummary("Block a time slot for a teacher")
            .WithDescription("Marks a specific time slot as unavailable for this teacher, with an optional reason.")
            .Produces<TeacherSlotBlockResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .WithSummary("Update a blocked slot")
            .WithDescription("Updates the reason for a blocked time slot.")
            .Produces<TeacherSlotBlockResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Unblock a time slot")
            .WithDescription("Removes the block, making this time slot available again for the teacher.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

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
