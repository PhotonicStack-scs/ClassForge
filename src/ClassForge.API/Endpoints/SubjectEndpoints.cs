using ClassForge.API.Filters;
using ClassForge.Application.DTOs.Subjects;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class SubjectEndpoints
{
    public static RouteGroupBuilder MapSubjectEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/subjects")
            .WithTags("Subjects")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create).AddEndpointFilter<ValidationFilter<CreateSubjectRequest>>();
        group.MapPut("/{id:guid}", Update).AddEndpointFilter<ValidationFilter<UpdateSubjectRequest>>();
        group.MapDelete("/{id:guid}", Delete);

        return group;
    }

    private static async Task<IResult> GetAll(IAppDbContext db)
    {
        var subjects = await db.Subjects.OrderBy(s => s.Name).ToListAsync();
        return Results.Ok(subjects.Select(s => s.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid id, IAppDbContext db)
    {
        var subject = await db.Subjects.FindAsync(id);
        return subject is null ? Results.NotFound() : Results.Ok(subject.ToResponse());
    }

    private static async Task<IResult> Create(
        CreateSubjectRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var subject = request.ToEntity(tenantId);
        db.Subjects.Add(subject);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/subjects/{subject.Id}", subject.ToResponse());
    }

    private static async Task<IResult> Update(Guid id, UpdateSubjectRequest request, IAppDbContext db)
    {
        var subject = await db.Subjects.FindAsync(id);
        if (subject is null) return Results.NotFound();

        subject.Name = request.Name;
        subject.RequiresSpecialRoom = request.RequiresSpecialRoom;
        subject.SpecialRoomId = request.SpecialRoomId;
        subject.MaxPeriodsPerDay = request.MaxPeriodsPerDay;
        subject.AllowDoublePeriods = request.AllowDoublePeriods;
        await db.SaveChangesAsync();

        return Results.Ok(subject.ToResponse());
    }

    private static async Task<IResult> Delete(Guid id, IAppDbContext db)
    {
        var subject = await db.Subjects.FindAsync(id);
        if (subject is null) return Results.NotFound();

        db.Subjects.Remove(subject);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
