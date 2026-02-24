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
            .WithDescription("Creates a new subject. Optionally link it to a special room. Scheduling constraints (max periods per day, double-period allowance) are configured per grade on the subject requirements.")
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

        group.MapPost("/bulk", BulkCreate)
            .AddEndpointFilter<ValidationFilter<BulkCreateSubjectsRequest>>()
            .WithSummary("Bulk create subjects")
            .Produces<List<SubjectResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

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
        subject.Color = request.Color ?? subject.Color;
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

    private static async Task<IResult> BulkCreate(
        BulkCreateSubjectsRequest request, ITenantProvider tenantProvider, IAppDbContext db)
    {
        if (tenantProvider.TenantId is not { } tenantId) return Results.Unauthorized();

        var entities = request.Items.Select(i => i.ToEntity(tenantId)).ToList();
        db.Subjects.AddRange(entities);
        await db.SaveChangesAsync();

        return Results.Created("/api/v1/subjects", entities.Select(s => s.ToResponse()).ToList());
    }
}
