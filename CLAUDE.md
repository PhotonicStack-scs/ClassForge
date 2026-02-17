# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
dotnet build ClassForge.sln               # Build entire solution
dotnet run --project src/ClassForge.API    # Run the API (requires PostgreSQL)
```

EF Core migrations (run from repo root):
```bash
dotnet ef migrations add <Name> --project src/ClassForge.Infrastructure --startup-project src/ClassForge.API
dotnet ef database update --project src/ClassForge.Infrastructure --startup-project src/ClassForge.API
```

Health check: `GET /health`
Swagger UI: `/swagger` (includes JWT Bearer authorize button)

## Architecture

Clean Architecture with four projects. Dependency flow: **Domain ← Application ← Infrastructure ← API**.

- **Domain** — Entities, enums, interfaces (`ITenantEntity`, `IAuditableEntity`). Zero dependencies.
- **Application** — DTOs (records), FluentValidation validators, `IAppDbContext`/`ITenantProvider`/`ITokenService` interfaces, mapping extensions. Depends on EF Core (for `DbSet<T>`) and Domain.
- **Infrastructure** — `AppDbContext`, EF configurations (in `Data/Configurations/`), `TokenService`, `TenantProvider`, `TenantInterceptor`. DI wired via `DependencyInjection.AddInfrastructure()`.
- **API** — `Program.cs`, Minimal API endpoint files (in `Endpoints/`), `ValidationFilter<T>`, `GlobalExceptionHandler`.

## Multi-Tenancy

Every tenant-scoped entity implements `ITenantEntity` (has `TenantId`). Isolation is enforced by:

1. **Global query filter** — `AppDbContext.OnModelCreating` dynamically applies `e.TenantId == tenantProvider.TenantId` to all `ITenantEntity` types.
2. **TenantInterceptor** — `SaveChangesInterceptor` that auto-stamps `TenantId` on new entities.
3. **TenantProvider** — Reads `tenant_id` claim from JWT via `IHttpContextAccessor`.

Auth endpoints (login, register, refresh) use `IgnoreQueryFilters()` since tenant context isn't established before authentication.

## Conventions

- **Endpoints**: Static classes with `MapXxxEndpoints()` extension method on `IEndpointRouteBuilder`. Each returns a `RouteGroupBuilder`. New endpoints must be registered in `Program.cs` via `app.MapXxxEndpoints()`.
- **DTOs**: Records in `Application/DTOs/{Feature}/`. Named `CreateXxxRequest`, `UpdateXxxRequest`, `XxxResponse`.
- **Mapping**: Static extension methods in `Application/Mapping/MappingExtensions.cs` — `ToResponse()` and `ToEntity()`. No AutoMapper.
- **Validation**: FluentValidation with `ValidationFilter<T>` endpoint filter. Validators in `Application/Validators/`.
- **Swagger**: Every endpoint has `.WithSummary()`, `.Produces<T>()`, and where applicable `.WithDescription()`, `.ProducesValidationProblem()`, `.ProducesProblem(StatusCodes.Status404NotFound)`. Maintain these when adding or modifying endpoints.
- **EF Configurations**: One `IEntityTypeConfiguration<T>` per entity in `Infrastructure/Data/Configurations/`. Unique indexes typically on `TenantId + Name`.
- **Authorization policies**: `"OrgAdmin"` and `"ScheduleManagerOrAbove"` (OrgAdmin + ScheduleManager).
- **Entity patterns**: All entities use `Guid` primary keys. Navigation properties assigned `= null!`. Collections initialized with `= []`. `TimeOnly` used for time slots.

## Database

PostgreSQL via Npgsql. Connection string in `appsettings.json` under `ConnectionStrings:DefaultConnection`. `AppDbContext` auto-sets `CreatedAt`/`UpdatedAt` on `IAuditableEntity` types during `SaveChangesAsync`.

## API Routes

All under `/api/v1/`. Auth routes are anonymous. Most resource routes require `ScheduleManagerOrAbove`. User management requires `OrgAdmin`. Sub-resources are nested (e.g., `/grades/{gradeId}/groups`, `/teachers/{teacherId}/qualifications`).

## Testing

No test project exists yet. Verify changes with `dotnet build ClassForge.sln` and manual testing via Swagger UI.

## Tech Stack

.NET 10 (SDK 10.0.101), PostgreSQL, EF Core, Serilog, FluentValidation, JWT Bearer auth, Swashbuckle (Swagger). `Directory.Build.props` sets `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, and `<LangVersion>latest</LangVersion>` for all projects.
