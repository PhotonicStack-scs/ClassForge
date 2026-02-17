using ClassForge.API.Filters;
using ClassForge.Application.DTOs.Users;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using ClassForge.Domain.Entities;
using ClassForge.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithSummary("List all users")
            .WithDescription("Returns all users for the current tenant, ordered by display name.")
            .Produces<List<UserResponse>>();

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a user by ID")
            .Produces<UserResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateUserRequest>>()
            .RequireAuthorization("OrgAdmin")
            .WithSummary("Create a user")
            .WithDescription("Creates a new user within the current tenant. Requires OrgAdmin role. Valid roles: OrgAdmin, ScheduleManager, Viewer.")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateUserRequest>>()
            .RequireAuthorization("OrgAdmin")
            .WithSummary("Update a user")
            .WithDescription("Updates the user's display name and role. Requires OrgAdmin role.")
            .Produces<UserResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .RequireAuthorization("OrgAdmin")
            .WithSummary("Delete a user")
            .WithDescription("Permanently removes a user from the tenant. Requires OrgAdmin role.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(IAppDbContext db)
    {
        var users = await db.Users.OrderBy(u => u.DisplayName).ToListAsync();
        return Results.Ok(users.Select(u => u.ToResponse()));
    }

    private static async Task<IResult> GetById(Guid id, IAppDbContext db)
    {
        var user = await db.Users.FindAsync(id);
        return user is null ? Results.NotFound() : Results.Ok(user.ToResponse());
    }

    private static async Task<IResult> Create(
        CreateUserRequest request,
        ITenantProvider tenantProvider,
        IAppDbContext db,
        IPasswordHasher<User> passwordHasher)
    {
        if (tenantProvider.TenantId is not { } tenantId)
            return Results.Unauthorized();

        if (!Enum.TryParse<UserRole>(request.Role, out var role))
            return Results.BadRequest(new { error = "Invalid role." });

        var exists = await db.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
            return Results.Conflict(new { error = "A user with this email already exists in this tenant." });

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = request.Email,
            DisplayName = request.DisplayName,
            Role = role
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/users/{user.Id}", user.ToResponse());
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateUserRequest request,
        IAppDbContext db)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
            return Results.NotFound();

        if (!Enum.TryParse<UserRole>(request.Role, out var role))
            return Results.BadRequest(new { error = "Invalid role." });

        user.DisplayName = request.DisplayName;
        user.Role = role;
        await db.SaveChangesAsync();

        return Results.Ok(user.ToResponse());
    }

    private static async Task<IResult> Delete(Guid id, IAppDbContext db)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
            return Results.NotFound();

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
