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

        group.MapGet("/", GetAll)
            .WithSummary("List all subjects")
            .WithDescription("Returns all subjects for the current tenant, ordered by name.")
            .Produces<List<SubjectResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a subject by ID")
            .Produces<SubjectResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateSubjectRequest>>()
            .WithSummary("Create a subject")
            .WithDescription("Creates a new subject. Optionally link it to a special room and configure scheduling constraints like max periods per day and double-period allowance.")
            .Produces<SubjectResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateSubjectRequest>>()
            .WithSummary("Update a subject")
            .Produces<SubjectResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Delete a subject")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

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
