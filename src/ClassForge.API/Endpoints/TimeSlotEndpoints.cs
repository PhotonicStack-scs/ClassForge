using ClassForge.API.Filters;
using ClassForge.Application.DTOs.TimeSlots;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class TimeSlotEndpoints
{
    public static RouteGroupBuilder MapTimeSlotEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/teaching-days/{dayId:guid}/time-slots")
            .WithTags("Time Slots")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create).AddEndpointFilter<ValidationFilter<CreateTimeSlotRequest>>();
        group.MapPut("/{id:guid}", Update).AddEndpointFilter<ValidationFilter<UpdateTimeSlotRequest>>();
        group.MapDelete("/{id:guid}", Delete);

        return group;
    }

    private static async Task<IResult> GetAll(Guid dayId, IAppDbContext db)
    {
        var slots = await db.TimeSlots
            .Where(s => s.TeachingDayId == dayId)
            .OrderBy(s => s.SlotNumber)
            .ToListAsync();
        return Results.Ok(slots.Select(s => s.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid dayId, Guid id, IAppDbContext db)
    {
        var slot = await db.TimeSlots
            .FirstOrDefaultAsync(s => s.Id == id && s.TeachingDayId == dayId);
        return slot is null ? Results.NotFound() : Results.Ok(slot.ToResponse());
    }

    private static async Task<IResult> Create(
        Guid dayId, CreateTimeSlotRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entity = request.ToEntity(tenantId, dayId);
        db.TimeSlots.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/teaching-days/{dayId}/time-slots/{entity.Id}", entity.ToResponse());
    }

    private static async Task<IResult> Update(Guid dayId, Guid id, UpdateTimeSlotRequest request, IAppDbContext db)
    {
        var slot = await db.TimeSlots
            .FirstOrDefaultAsync(s => s.Id == id && s.TeachingDayId == dayId);
        if (slot is null) return Results.NotFound();

        slot.SlotNumber = request.SlotNumber;
        slot.StartTime = TimeOnly.Parse(request.StartTime);
        slot.EndTime = TimeOnly.Parse(request.EndTime);
        slot.IsBreak = request.IsBreak;
        await db.SaveChangesAsync();

        return Results.Ok(slot.ToResponse());
    }

    private static async Task<IResult> Delete(Guid dayId, Guid id, IAppDbContext db)
    {
        var slot = await db.TimeSlots
            .FirstOrDefaultAsync(s => s.Id == id && s.TeachingDayId == dayId);
        if (slot is null) return Results.NotFound();

        db.TimeSlots.Remove(slot);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
