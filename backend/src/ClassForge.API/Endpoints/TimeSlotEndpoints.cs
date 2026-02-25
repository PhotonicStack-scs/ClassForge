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
        var group = routes.MapGroup("/api/v1/school-days/{dayId:guid}/time-slots")
            .WithTags("Time Slots")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll)
            .WithSummary("List time slots for a school day")
            .WithDescription("Returns all time slots for the specified school day, ordered by slot number. Includes both lesson slots and breaks.")
            .Produces<List<TimeSlotResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a time slot by ID")
            .Produces<TimeSlotResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateTimeSlotRequest>>()
            .WithSummary("Create a time slot")
            .WithDescription("Creates a new time slot within a school day. Specify start/end times (HH:mm) and whether this slot is a break.")
            .Produces<TimeSlotResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateTimeSlotRequest>>()
            .WithSummary("Update a time slot")
            .Produces<TimeSlotResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Delete a time slot")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/bulk", BulkCreate)
            .AddEndpointFilter<ValidationFilter<BulkCreateTimeSlotsRequest>>()
            .WithSummary("Bulk create time slots")
            .Produces<List<TimeSlotResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        return group;
    }

    private static async Task<IResult> GetAll(Guid dayId, IAppDbContext db)
    {
        var slots = await db.TimeSlots
            .Where(s => s.SchoolDayId == dayId)
            .OrderBy(s => s.SlotNumber)
            .ToListAsync();
        return Results.Ok(slots.Select(s => s.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid dayId, Guid id, IAppDbContext db)
    {
        var slot = await db.TimeSlots
            .FirstOrDefaultAsync(s => s.Id == id && s.SchoolDayId == dayId);
        return slot is null ? Results.NotFound() : Results.Ok(slot.ToResponse());
    }

    private static async Task<IResult> Create(
        Guid dayId, CreateTimeSlotRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entity = request.ToEntity(tenantId, dayId);
        db.TimeSlots.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/school-days/{dayId}/time-slots/{entity.Id}", entity.ToResponse());
    }

    private static async Task<IResult> Update(Guid dayId, Guid id, UpdateTimeSlotRequest request, IAppDbContext db)
    {
        var slot = await db.TimeSlots
            .FirstOrDefaultAsync(s => s.Id == id && s.SchoolDayId == dayId);
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
            .FirstOrDefaultAsync(s => s.Id == id && s.SchoolDayId == dayId);
        if (slot is null) return Results.NotFound();

        db.TimeSlots.Remove(slot);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> BulkCreate(
        Guid dayId, BulkCreateTimeSlotsRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entities = request.Items.Select(i => i.ToEntity(tenantId, dayId)).ToList();
        db.TimeSlots.AddRange(entities);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/school-days/{dayId}/time-slots", entities.Select(s => s.ToResponse()).ToList());
    }
}
