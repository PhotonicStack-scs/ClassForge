using System.Security.Claims;
using ClassForge.API.Filters;
using ClassForge.Application.DTOs.Timetables;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using ClassForge.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class TimetableEndpoints
{
    public static RouteGroupBuilder MapTimetableEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/timetables")
            .WithTags("Timetables")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapPost("/preflight", RunPreflight)
            .WithSummary("Run pre-flight validation")
            .WithDescription("Validates all configuration data before timetable generation.")
            .Produces<PreflightResponse>();

        group.MapGet("/", GetAll)
            .WithSummary("List all timetables")
            .Produces<List<TimetableResponse>>();

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateTimetableRequest>>()
            .WithSummary("Create and generate a timetable")
            .WithDescription("Creates a timetable record in Generating status and enqueues async generation. Poll GET /{id} for status.")
            .Produces<TimetableResponse>(StatusCodes.Status202Accepted)
            .ProducesValidationProblem();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a timetable by ID")
            .WithDescription("Returns timetable with current status. Use for polling generation progress.")
            .Produces<TimetableResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateTimetableRequest>>()
            .WithSummary("Update a timetable name")
            .Produces<TimetableResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithSummary("Delete a timetable")
            .WithDescription("Permanently removes a timetable and all its entries and reports.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/entries", GetEntries)
            .WithSummary("Get timetable entries")
            .WithDescription("Returns entries, optionally filtered by groupId, teacherId, or teachingDayId query parameters.")
            .Produces<List<TimetableEntryResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/report", GetReport)
            .WithSummary("Get timetable quality report")
            .Produces<List<TimetableReportResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/publish", Publish)
            .WithSummary("Publish a timetable")
            .WithDescription("Changes status from Draft to Published.")
            .Produces<TimetableResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/validate", ValidateEntries)
            .WithSummary("Validate all timetable entries")
            .WithDescription("Re-validates all entries against hard constraints and returns any violations.")
            .Produces<List<string>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/by-group/{groupId:guid}", GetByGroup)
            .WithSummary("Get weekly view for a group")
            .Produces<TimetableViewResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/by-teacher/{teacherId:guid}", GetByTeacher)
            .WithSummary("Get weekly view for a teacher")
            .Produces<TimetableViewResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/by-room/{roomId:guid}", GetByRoom)
            .WithSummary("Get weekly view for a room")
            .Produces<TimetableViewResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/entries/{entryId:guid}", UpdateEntry)
            .AddEndpointFilter<ValidationFilter<UpdateTimetableEntryRequest>>()
            .WithSummary("Update a timetable entry")
            .WithDescription("Updates a single entry in a Draft timetable. Validates against hard constraints; returns 409 if violations found.")
            .Produces<TimetableEntryResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();

        return group;
    }

    private static async Task<IResult> RunPreflight(IPreflightValidator validator, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetAll(IAppDbContext db)
    {
        var timetables = await db.Timetables.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return Results.Ok(timetables.Select(t => t.ToResponse()));
    }

    private static async Task<IResult> Create(
        CreateTimetableRequest request,
        ITenantProvider tenantProvider,
        ITimetableGenerationQueue queue,
        IAppDbContext db,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (tenantProvider.TenantId is not { } tenantId)
            return Results.Unauthorized();

        var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var timetable = new Domain.Entities.Timetable
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Status = TimetableStatus.Generating,
            CreatedBy = userId
        };

        db.Timetables.Add(timetable);
        await db.SaveChangesAsync(ct);

        await queue.EnqueueAsync(new TimetableGenerationRequest(timetable.Id, tenantId), ct);

        return Results.Accepted($"/api/v1/timetables/{timetable.Id}", timetable.ToResponse());
    }

    private static async Task<IResult> GetById(Guid id, IAppDbContext db)
    {
        var timetable = await db.Timetables.FirstOrDefaultAsync(t => t.Id == id);
        return timetable is null ? Results.NotFound() : Results.Ok(timetable.ToResponse());
    }

    private static async Task<IResult> Update(Guid id, UpdateTimetableRequest request, IAppDbContext db)
    {
        var timetable = await db.Timetables.FirstOrDefaultAsync(t => t.Id == id);
        if (timetable is null) return Results.NotFound();

        timetable.Name = request.Name;
        await db.SaveChangesAsync();

        return Results.Ok(timetable.ToResponse());
    }

    private static async Task<IResult> Delete(Guid id, IAppDbContext db)
    {
        var timetable = await db.Timetables.FirstOrDefaultAsync(t => t.Id == id);
        if (timetable is null) return Results.NotFound();

        db.Timetables.Remove(timetable);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> GetEntries(
        Guid id, IAppDbContext db,
        Guid? groupId = null, Guid? teacherId = null, Guid? teachingDayId = null)
    {
        var timetable = await db.Timetables.FirstOrDefaultAsync(t => t.Id == id);
        if (timetable is null) return Results.NotFound();

        var query = db.TimetableEntries
            .Include(e => e.Groups)
            .Where(e => e.TimetableId == id);

        if (groupId.HasValue)
            query = query.Where(e => e.Groups.Any(g => g.GroupId == groupId.Value));

        if (teacherId.HasValue)
            query = query.Where(e => e.TeacherId == teacherId.Value);

        if (teachingDayId.HasValue)
        {
            var slotIds = await db.TimeSlots
                .Where(s => s.TeachingDayId == teachingDayId.Value)
                .Select(s => s.Id)
                .ToListAsync();
            query = query.Where(e => slotIds.Contains(e.TimeSlotId));
        }

        var entries = await query.ToListAsync();
        return Results.Ok(entries.Select(e => e.ToResponse()));
    }

    private static async Task<IResult> GetReport(Guid id, IAppDbContext db)
    {
        var timetable = await db.Timetables.FirstOrDefaultAsync(t => t.Id == id);
        if (timetable is null) return Results.NotFound();

        var reports = await db.TimetableReports
            .Where(r => r.TimetableId == id)
            .ToListAsync();

        return Results.Ok(reports.Select(r => r.ToResponse()));
    }

    private static async Task<IResult> Publish(Guid id, IAppDbContext db)
    {
        var timetable = await db.Timetables.FirstOrDefaultAsync(t => t.Id == id);
        if (timetable is null) return Results.NotFound();

        if (timetable.Status != TimetableStatus.Draft)
            return Results.Problem(
                detail: $"Only Draft timetables can be published. Current status: {timetable.Status}.",
                statusCode: StatusCodes.Status400BadRequest);

        timetable.Status = TimetableStatus.Published;
        await db.SaveChangesAsync();

        return Results.Ok(timetable.ToResponse());
    }

    private static async Task<IResult> ValidateEntries(
        Guid id, IAppDbContext db, ITimetableEntryValidator validator, CancellationToken ct)
    {
        var timetable = await db.Timetables.FirstOrDefaultAsync(t => t.Id == id);
        if (timetable is null) return Results.NotFound();

        var entries = await db.TimetableEntries
            .Include(e => e.Groups)
            .Where(e => e.TimetableId == id)
            .ToListAsync(ct);

        var allIssues = new List<string>();
        foreach (var entry in entries)
        {
            var issues = await validator.ValidateEntryAsync(id, entry, ct);
            allIssues.AddRange(issues);
        }

        return Results.Ok(allIssues);
    }

    private static async Task<IResult> GetByGroup(Guid id, Guid groupId, IAppDbContext db)
    {
        var timetable = await db.Timetables.FirstOrDefaultAsync(t => t.Id == id);
        if (timetable is null) return Results.NotFound();

        var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
        if (group is null) return Results.NotFound();

        var entries = await BuildViewEntries(db, id, e => e.Groups.Any(g => g.GroupId == groupId));
        return Results.Ok(new TimetableViewResponse(id, "Group", group.Name, entries));
    }

    private static async Task<IResult> GetByTeacher(Guid id, Guid teacherId, IAppDbContext db)
    {
        var timetable = await db.Timetables.FirstOrDefaultAsync(t => t.Id == id);
        if (timetable is null) return Results.NotFound();

        var teacher = await db.Teachers.FirstOrDefaultAsync(t => t.Id == teacherId);
        if (teacher is null) return Results.NotFound();

        var entries = await BuildViewEntries(db, id, e => e.TeacherId == teacherId);
        return Results.Ok(new TimetableViewResponse(id, "Teacher", teacher.Name, entries));
    }

    private static async Task<IResult> GetByRoom(Guid id, Guid roomId, IAppDbContext db)
    {
        var timetable = await db.Timetables.FirstOrDefaultAsync(t => t.Id == id);
        if (timetable is null) return Results.NotFound();

        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);
        if (room is null) return Results.NotFound();

        var entries = await BuildViewEntries(db, id, e => e.RoomId == roomId);
        return Results.Ok(new TimetableViewResponse(id, "Room", room.Name, entries));
    }

    private static async Task<List<TimetableViewEntry>> BuildViewEntries(
        IAppDbContext db, Guid timetableId,
        System.Linq.Expressions.Expression<Func<Domain.Entities.TimetableEntry, bool>> filter)
    {
        var entries = await db.TimetableEntries
            .Include(e => e.TimeSlot).ThenInclude(s => s.TeachingDay)
            .Include(e => e.Subject)
            .Include(e => e.Teacher)
            .Include(e => e.Room)
            .Include(e => e.Groups).ThenInclude(g => g.Group)
            .Where(e => e.TimetableId == timetableId)
            .Where(filter)
            .ToListAsync();

        return entries.Select(e => new TimetableViewEntry(
            e.Id,
            e.TimeSlot.TeachingDay.DayOfWeek,
            e.TimeSlot.SlotNumber,
            e.Subject.Name,
            e.Teacher.Name,
            e.Room?.Name,
            e.IsDoublePeriod,
            e.Groups.Select(g => g.Group.Name).ToList()
        )).ToList();
    }

    private static async Task<IResult> UpdateEntry(
        Guid id, Guid entryId,
        UpdateTimetableEntryRequest request,
        IAppDbContext db,
        ITimetableEntryValidator validator,
        CancellationToken ct)
    {
        var timetable = await db.Timetables.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (timetable is null) return Results.NotFound();

        if (timetable.Status != TimetableStatus.Draft)
            return Results.Problem(
                detail: $"Only Draft timetables can be edited. Current status: {timetable.Status}.",
                statusCode: StatusCodes.Status400BadRequest);

        var entry = await db.TimetableEntries
            .Include(e => e.Groups)
            .FirstOrDefaultAsync(e => e.Id == entryId && e.TimetableId == id, ct);
        if (entry is null) return Results.NotFound();

        // Update entry fields
        entry.TimeSlotId = request.TimeSlotId;
        entry.SubjectId = request.SubjectId;
        entry.TeacherId = request.TeacherId;
        entry.RoomId = request.RoomId;
        entry.IsDoublePeriod = request.IsDoublePeriod;

        // Replace groups
        db.TimetableEntryGroups.RemoveRange(entry.Groups);
        entry.Groups = request.GroupIds.Select(gid => new Domain.Entities.TimetableEntryGroup
        {
            TimetableEntryId = entry.Id,
            GroupId = gid
        }).ToList();

        // Validate
        var issues = await validator.ValidateEntryAsync(id, entry, ct);
        if (issues.Count > 0)
            return Results.Conflict(new { Violations = issues });

        await db.SaveChangesAsync(ct);
        return Results.Ok(entry.ToResponse());
    }
}
