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

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create).AddEndpointFilter<ValidationFilter<CreateRoomRequest>>();
        group.MapPut("/{id:guid}", Update).AddEndpointFilter<ValidationFilter<UpdateRoomRequest>>();
        group.MapDelete("/{id:guid}", Delete);

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
