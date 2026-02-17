# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build, Run & Test

```bash
dotnet build ClassForge.sln               # Build entire solution (7 projects)
dotnet run --project src/ClassForge.API    # Run the API (requires PostgreSQL)
dotnet test                                # Run all tests
dotnet test tests/ClassForge.Tests.Unit    # Unit tests only (no Docker needed)
dotnet test tests/ClassForge.Tests.Integration  # Integration tests (requires Docker)
dotnet test --filter "FullyQualifiedName~HardConstraintTests.HC1"  # Single test by name
```

EF Core migrations (run from repo root):
```bash
dotnet ef migrations add <Name> --project src/ClassForge.Infrastructure --startup-project src/ClassForge.API
dotnet ef database update --project src/ClassForge.Infrastructure --startup-project src/ClassForge.API
```

Health check: `GET /health` | Swagger UI: `/swagger` (includes JWT Bearer authorize button)

In Development mode, `SeedData.SeedDemoSchoolAsync` auto-seeds a demo school on startup.

## Architecture

Clean Architecture with five source projects. Dependency flow:

```
Domain (no deps)
  ^
Application (Domain + EF Core for DbSet<T>)
  ^            ^
Infrastructure  Scheduling
  ^       ^       ^
  +--- API -------+
```

- **Domain** — Entities (20), enums (`UserRole`, `TimetableStatus`, `ReportType`), interfaces (`ITenantEntity`, `IAuditableEntity`). Zero dependencies.
- **Application** — DTOs (records), FluentValidation validators, interfaces (`IAppDbContext`, `ITenantProvider`, `ITokenService`, `ITimetableGenerator`, `IPreflightValidator`, `ITimetableEntryValidator`, `ITimetableGenerationQueue`), mapping extensions.
- **Infrastructure** — `AppDbContext`, EF configurations (`Data/Configurations/`), services (`Services/`). DI wired via `DependencyInjection.AddInfrastructure()`. Contains: `TenantProvider`, `TokenService`, `PreflightValidator`, `TimetableEntryValidator`, `SchedulingInputBuilder`, `TimetableGenerationQueue` (Channel&lt;T&gt;), `TimetableGenerationService` (BackgroundService).
- **Scheduling** — Timetable generation engine. Depends only on Application (no EF/Infrastructure). Contains: `TimetableGenerator` (implements `ITimetableGenerator`), `ConstraintPropagation`, `BacktrackingSolver`, `HardConstraintChecker`, `SoftConstraintScorer`, `ReportGenerator`. Registered in `Program.cs`, not `AddInfrastructure()`.
- **API** — `Program.cs`, 17 endpoint files (`Endpoints/`), `ValidationFilter<T>`, `GlobalExceptionHandler`.

**Test projects:**
- **Tests.Unit** — xUnit v3, FluentAssertions, NSubstitute. Tests scheduling constraints, solver, and propagation. `TestDataBuilder` creates minimal `SchedulingInput` fixtures.
- **Tests.Integration** — xUnit v3, Testcontainers PostgreSQL, Mvc.Testing. `CustomWebApplicationFactory` replaces DbContext with Testcontainers and removes the `TimetableGenerationService` hosted service.

## Multi-Tenancy

Every tenant-scoped entity implements `ITenantEntity` (has `TenantId`). Isolation is enforced by:

1. **Global query filter** — `AppDbContext.OnModelCreating` dynamically applies `e.TenantId == tenantProvider.TenantId` to all `ITenantEntity` types via expression trees.
2. **TenantInterceptor** — `SaveChangesInterceptor` that auto-stamps `TenantId` on new entities.
3. **TenantProvider** — Reads `tenant_id` claim from JWT via `IHttpContextAccessor`. Also supports `SetTenantId(Guid)` override for background services that lack HTTP context.

Auth endpoints (login, register, refresh) use `IgnoreQueryFilters()` since tenant context isn't established before authentication.

## Async Timetable Generation

Timetable creation is asynchronous: `POST /api/v1/timetables` creates a record with status `Generating`, enqueues a `TimetableGenerationRequest` onto a `Channel<T>`, and returns 202. The `TimetableGenerationService` (BackgroundService) reads from the channel, creates a DI scope, sets the tenant ID on `TenantProvider`, builds a `SchedulingInput` snapshot, runs `ITimetableGenerator.GenerateAsync`, and persists entries + reports. Clients poll `GET /{id}` until status changes to `Draft` or `Failed`.

## Conventions

- **Endpoints**: Static classes with `MapXxxEndpoints()` extension method on `IEndpointRouteBuilder`. Each returns a `RouteGroupBuilder`. New endpoints must be registered in `Program.cs` via `app.MapXxxEndpoints()`.
- **DTOs**: Records in `Application/DTOs/{Feature}/`. Named `CreateXxxRequest`, `UpdateXxxRequest`, `XxxResponse`.
- **Mapping**: Static extension methods in `Application/Mapping/MappingExtensions.cs` — `ToResponse()` and `ToEntity()`. No AutoMapper.
- **Validation**: FluentValidation with `ValidationFilter<T>` endpoint filter. Validators in `Application/Validators/` are auto-registered via `AddValidatorsFromAssemblyContaining`.
- **Swagger**: Every endpoint has `.WithSummary()`, `.Produces<T>()`, and where applicable `.WithDescription()`, `.ProducesValidationProblem()`, `.ProducesProblem(StatusCodes.Status404NotFound)`. Maintain these when adding or modifying endpoints.
- **EF Configurations**: One `IEntityTypeConfiguration<T>` per entity in `Infrastructure/Data/Configurations/`. Unique indexes typically on `TenantId + Name`. Enum properties use `.HasConversion<string>()`.
- **Authorization policies**: `"OrgAdmin"` and `"ScheduleManagerOrAbove"` (OrgAdmin + ScheduleManager).
- **Entity patterns**: All entities use `Guid` primary keys. Navigation properties assigned `= null!`. Collections initialized with `= []`. `TimeOnly` used for time slots. Tenant-scoped entities implement `ITenantEntity`. Auditable entities implement `IAuditableEntity`. Child-of-tenant entities (e.g., `TeacherSubjectQualification`) may only implement `IAuditableEntity` — their tenancy is enforced via their parent's query filter.
- **Sub-resource endpoints**: Verify parent existence via `AnyAsync` before proceeding. Filter by both parent ID and item ID.

## Database

PostgreSQL via Npgsql. Connection string in `appsettings.json` under `ConnectionStrings:DefaultConnection`. `AppDbContext` auto-sets `CreatedAt`/`UpdatedAt` on `IAuditableEntity` types during `SaveChangesAsync`. New `DbSet` properties must be added to both `IAppDbContext` (Application) and `AppDbContext` (Infrastructure).

## API Routes

All under `/api/v1/`. Auth routes are anonymous. Most resource routes require `ScheduleManagerOrAbove`. User management requires `OrgAdmin`. Sub-resources are nested (e.g., `/grades/{gradeId}/groups`, `/teachers/{teacherId}/qualifications`). Timetable endpoints include CRUD, entries (filterable), report, publish, validate, and view-by (group/teacher/room).

## Tech Stack

.NET 10 (SDK 10.0.101), PostgreSQL, EF Core, Serilog, FluentValidation, JWT Bearer auth, Swashbuckle (Swagger), xUnit v3, FluentAssertions, NSubstitute, Testcontainers. `Directory.Build.props` sets `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, and `<LangVersion>latest</LangVersion>` for all projects.
