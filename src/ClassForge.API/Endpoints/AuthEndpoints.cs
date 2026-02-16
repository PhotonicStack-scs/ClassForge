using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ClassForge.API.Filters;
using ClassForge.Application.DTOs.Auth;
using ClassForge.Application.Interfaces;
using ClassForge.Application.Mapping;
using ClassForge.Domain.Entities;
using ClassForge.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClassForge.API.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/register", Register)
            .AddEndpointFilter<ValidationFilter<RegisterRequest>>()
            .AllowAnonymous();

        group.MapPost("/login", Login)
            .AddEndpointFilter<ValidationFilter<LoginRequest>>()
            .AllowAnonymous();

        group.MapPost("/refresh", Refresh)
            .AllowAnonymous();

        group.MapGet("/me", Me)
            .RequireAuthorization();

        group.MapGet("/oauth/{provider}", OAuthChallenge)
            .AllowAnonymous();

        group.MapGet("/oauth/{provider}/callback", OAuthCallback)
            .AllowAnonymous();

        return group;
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        IAppDbContext db,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokenService)
    {
        // Check if email already exists (ignore tenant filter)
        var existingUser = await db.Users.IgnoreQueryFilters()
            .AnyAsync(u => u.Email == request.Email);

        if (existingUser)
            return Results.Conflict(new { error = "A user with this email already exists." });

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.SchoolName
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = request.Email,
            DisplayName = request.DisplayName,
            Role = UserRole.OrgAdmin
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        var refreshToken = tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        db.Tenants.Add(tenant);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var accessToken = tokenService.GenerateAccessToken(user);

        return Results.Created("/api/v1/auth/me", new AuthResponse(
            accessToken, refreshToken, DateTime.UtcNow.AddHours(1)));
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IAppDbContext db,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokenService)
    {
        var user = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || user.PasswordHash is null)
            return Results.Unauthorized();

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Results.Unauthorized();

        var refreshToken = tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await db.SaveChangesAsync();

        var accessToken = tokenService.GenerateAccessToken(user);

        return Results.Ok(new AuthResponse(accessToken, refreshToken, DateTime.UtcNow.AddHours(1)));
    }

    private static async Task<IResult> Refresh(
        RefreshTokenRequest request,
        IAppDbContext db,
        ITokenService tokenService)
    {
        var userId = tokenService.GetUserIdFromExpiredToken(request.AccessToken);
        if (userId is null)
            return Results.Unauthorized();

        var user = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return Results.Unauthorized();

        var newAccessToken = tokenService.GenerateAccessToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await db.SaveChangesAsync();

        return Results.Ok(new AuthResponse(newAccessToken, newRefreshToken, DateTime.UtcNow.AddHours(1)));
    }

    private static async Task<IResult> Me(
        HttpContext httpContext,
        IAppDbContext db)
    {
        var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Results.Unauthorized();

        var user = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return Results.NotFound();

        return Results.Ok(user.ToProfileResponse());
    }

    private static IResult OAuthChallenge(string provider)
    {
        return Results.Ok(new { message = $"OAuth with {provider} - redirect to external provider (stub)" });
    }

    private static IResult OAuthCallback(string provider)
    {
        return Results.Ok(new { message = $"OAuth callback for {provider} (stub)" });
    }
}
