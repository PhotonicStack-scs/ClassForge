using ClassForge.API.Filters;
using ClassForge.Application.DTOs.Rooms;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class RoomEndpoints
{
    public static RouteGroupBuilder MapRoomEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/rooms")
            .WithTags("Rooms")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll)
            .WithSummary("List all rooms")
            .WithDescription("Returns all rooms for the current tenant, ordered by name.")
            .Produces<List<RoomResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a room by ID")
            .Produces<RoomResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateRoomRequest>>()
            .WithSummary("Create a room")
            .WithDescription("Creates a new room. Rooms can be referenced by subjects that require a special room (e.g. science lab, gym).")
            .Produces<RoomResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateRoomRequest>>()
            .WithSummary("Update a room")
            .Produces<RoomResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Delete a room")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(IAppDbContext db)
    {
        var rooms = await db.Rooms.OrderBy(r => r.Name).ToListAsync();
        return Results.Ok(rooms.Select(r => r.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid id, IAppDbContext db)
    {
        var room = await db.Rooms.FindAsync(id);
        return room is null ? Results.NotFound() : Results.Ok(room.ToResponse());
    }

    private static async Task<IResult> Create(
        CreateRoomRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var room = request.ToEntity(tenantId);
        db.Rooms.Add(room);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/rooms/{room.Id}", room.ToResponse());
    }

    private static async Task<IResult> Update(Guid id, UpdateRoomRequest request, IAppDbContext db)
    {
        var room = await db.Rooms.FindAsync(id);
        if (room is null) return Results.NotFound();

        room.Name = request.Name;
        room.Capacity = request.Capacity;
        await db.SaveChangesAsync();

        return Results.Ok(room.ToResponse());
    }

    private static async Task<IResult> Delete(Guid id, IAppDbContext db)
    {
        var room = await db.Rooms.FindAsync(id);
        if (room is null) return Results.NotFound();

        db.Rooms.Remove(room);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
