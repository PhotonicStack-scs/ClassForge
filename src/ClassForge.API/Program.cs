using System.Security.Claims;
using System.Text;
using ClassForge.API.Endpoints;
using ClassForge.API.Middleware;
using ClassForge.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console();
});

// Infrastructure (EF, services)
builder.Services.AddInfrastructure(builder.Configuration);

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ClassForge.Application.Validators.RegisterRequestValidator>();

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            RoleClaimType = ClaimTypes.Role
        };
    });

// OAuth stubs (configured but providers need real client IDs)
if (!string.IsNullOrEmpty(builder.Configuration["Authentication:Google:ClientId"]))
{
    builder.Services.AddAuthentication().AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    });
}

if (!string.IsNullOrEmpty(builder.Configuration["Authentication:Microsoft:ClientId"]))
{
    builder.Services.AddAuthentication().AddMicrosoftAccount(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
    });
}

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OrgAdmin", policy =>
        policy.RequireRole("OrgAdmin"));

    options.AddPolicy("ScheduleManagerOrAbove", policy =>
        policy.RequireRole("OrgAdmin", "ScheduleManager"));
});

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

// Health check
app.MapHealthChecks("/health");

// Endpoints
app.MapAuthEndpoints();
app.MapTenantEndpoints();
app.MapUserEndpoints();
app.MapGradeEndpoints();
app.MapGroupEndpoints();
app.MapSubjectEndpoints();
app.MapRoomEndpoints();
app.MapGradeSubjectRequirementEndpoints();
app.MapCombinedLessonEndpoints();
app.MapTeachingDayEndpoints();
app.MapTimeSlotEndpoints();
app.MapGradeDayConfigEndpoints();
app.MapTeacherEndpoints();
app.MapTeacherQualificationEndpoints();
app.MapTeacherDayConfigEndpoints();
app.MapTeacherSlotBlockEndpoints();

app.Run();
