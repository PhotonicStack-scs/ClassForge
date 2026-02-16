using ClassForge.API.Filters;
using ClassForge.Application.DTOs.TeachingDays;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class TeachingDayEndpoints
{
    public static RouteGroupBuilder MapTeachingDayEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/teaching-days")
            .WithTags("Teaching Days")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create).AddEndpointFilter<ValidationFilter<CreateTeachingDayRequest>>();
        group.MapPut("/{id:guid}", Update).AddEndpointFilter<ValidationFilter<UpdateTeachingDayRequest>>();
        group.MapDelete("/{id:guid}", Delete);

        return group;
    }

    private static async Task<IResult> GetAll(IAppDbContext db)
    {
        var days = await db.TeachingDays.OrderBy(d => d.SortOrder).ToListAsync();
        return Results.Ok(days.Select(d => d.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid id, IAppDbContext db)
    {
        var day = await db.TeachingDays.FindAsync(id);
        return day is null ? Results.NotFound() : Results.Ok(day.ToResponse());
    }

    private static async Task<IResult> Create(
        CreateTeachingDayRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entity = request.ToEntity(tenantId);
        db.TeachingDays.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/teaching-days/{entity.Id}", entity.ToResponse());
    }

    private static async Task<IResult> Update(Guid id, UpdateTeachingDayRequest request, IAppDbContext db)
    {
        var entity = await db.TeachingDays.FindAsync(id);
        if (entity is null) return Results.NotFound();

        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;
        await db.SaveChangesAsync();

        return Results.Ok(entity.ToResponse());
    }

    private static async Task<IResult> Delete(Guid id, IAppDbContext db)
    {
        var entity = await db.TeachingDays.FindAsync(id);
        if (entity is null) return Results.NotFound();

        db.TeachingDays.Remove(entity);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
