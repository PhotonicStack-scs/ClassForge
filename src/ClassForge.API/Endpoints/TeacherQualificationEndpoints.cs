using ClassForge.API.Filters;
using ClassForge.Application.DTOs.TeacherQualifications;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class TeacherQualificationEndpoints
{
    public static RouteGroupBuilder MapTeacherQualificationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/teachers/{teacherId:guid}/qualifications")
            .WithTags("Teacher Qualifications")
            .RequireAuthorization("ScheduleManagerOrAbove");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create).AddEndpointFilter<ValidationFilter<CreateTeacherQualificationRequest>>();
        group.MapPut("/{id:guid}", Update).AddEndpointFilter<ValidationFilter<UpdateTeacherQualificationRequest>>();
        group.MapDelete("/{id:guid}", Delete);

        return group;
    }

    private static async Task<IResult> VerifyTeacher(Guid teacherId, IAppDbContext db)
    {
        var exists = await db.Teachers.AnyAsync(t => t.Id == teacherId);
        return exists ? null! : Results.NotFound(new { error = "Teacher not found." });
    }

    private static async Task<IResult> GetAll(Guid teacherId, IAppDbContext db)
    {
        var check = await VerifyTeacher(teacherId, db);
        if (check is not null) return check;

        var qualifications = await db.TeacherSubjectQualifications
            .Where(q => q.TeacherId == teacherId)
            .ToListAsync();
        return Results.Ok(qualifications.Select(q => q.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid teacherId, Guid id, IAppDbContext db)
    {
        var check = await VerifyTeacher(teacherId, db);
        if (check is not null) return check;

        var q = await db.TeacherSubjectQualifications
            .FirstOrDefaultAsync(q => q.Id == id && q.TeacherId == teacherId);
        return q is null ? Results.NotFound() : Results.Ok(q.ToResponse());
    }

    private static async Task<IResult> Create(
        Guid teacherId, CreateTeacherQualificationRequest request, IAppDbContext db)
    {
        var check = await VerifyTeacher(teacherId, db);
        if (check is not null) return check;

        var entity = request.ToEntity(teacherId);
        db.TeacherSubjectQualifications.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/teachers/{teacherId}/qualifications/{entity.Id}", entity.ToResponse());
    }

    private static async Task<IResult> Update(
        Guid teacherId, Guid id, UpdateTeacherQualificationRequest request, IAppDbContext db)
    {
        var entity = await db.TeacherSubjectQualifications
            .FirstOrDefaultAsync(q => q.Id == id && q.TeacherId == teacherId);
        if (entity is null) return Results.NotFound();

        entity.SubjectId = request.SubjectId;
        entity.MinGradeId = request.MinGradeId;
        entity.MaxGradeId = request.MaxGradeId;
        await db.SaveChangesAsync();

        return Results.Ok(entity.ToResponse());
    }

    private static async Task<IResult> Delete(Guid teacherId, Guid id, IAppDbContext db)
    {
        var entity = await db.TeacherSubjectQualifications
            .FirstOrDefaultAsync(q => q.Id == id && q.TeacherId == teacherId);
        if (entity is null) return Results.NotFound();

        db.TeacherSubjectQualifications.Remove(entity);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
