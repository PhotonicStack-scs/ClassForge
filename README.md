# ClassForge

A multi-tenant SaaS backend that automatically generates weekly class timetables for schools. Schools define their structure (grades, groups, rooms), teachers (availability, qualifications), and curriculum requirements, then ClassForge uses a constraint-satisfaction algorithm to produce optimized weekly schedules.

## Tech Stack

- **Runtime:** .NET 10 (C#)
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core
- **API:** ASP.NET Core Minimal APIs
- **Auth:** JWT Bearer + OAuth2 (Google, Microsoft)
- **Logging:** Serilog
- **Validation:** FluentValidation
- **Architecture:** Clean Architecture

## Prerequisites

- [.NET SDK 10.0.101](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (latest stable)

## Getting Started

### 1. Clone and restore

```bash
git clone <repo-url>
cd ClassForge
dotnet restore ClassForge.slnx
```

### 2. Configure the database

Update the connection string in `src/ClassForge.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=classforge;Username=postgres;Password=postgres"
  }
}
```

### 3. Apply migrations

```bash
dotnet ef database update --project src/ClassForge.Infrastructure --startup-project src/ClassForge.API
```

### 4. Run

```bash
dotnet run --project src/ClassForge.API
```

The API starts at `https://localhost:5001` (or `http://localhost:5000`). Verify with `GET /health`.

## Configuration

### JWT

Configure in `appsettings.json` under the `Jwt` section. For production, replace the default key with a secure secret via environment variables or user secrets:

```bash
dotnet user-secrets set "Jwt:Key" "your-production-secret-key-at-least-32-chars" --project src/ClassForge.API
```

### OAuth Providers

Add Google and Microsoft client credentials in `appsettings.json` under `Authentication:Google` and `Authentication:Microsoft`. Leave empty to disable.

## Project Structure

```
src/
  ClassForge.Domain/            Entities, enums, interfaces. Zero dependencies.
  ClassForge.Application/       DTOs, validators, interfaces, mapping extensions.
  ClassForge.Infrastructure/    EF Core DbContext, configurations, services.
  ClassForge.API/               Program.cs, endpoints, middleware, filters.
```

Dependency flow: Domain <- Application <- Infrastructure <- API

## API Overview

All endpoints are under `/api/v1/`. Authentication uses JWT Bearer tokens.

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/register` | Register a new school + admin user |
| POST | `/api/v1/auth/login` | Login, returns JWT + refresh token |
| POST | `/api/v1/auth/refresh` | Refresh an expired access token |
| GET | `/api/v1/auth/me` | Get current user profile |

### School & Users

| Method | Endpoint | Auth |
|--------|----------|------|
| GET/PUT | `/api/v1/school` | Any authenticated / OrgAdmin |
| CRUD | `/api/v1/users` | OrgAdmin |

### Academic Structure

| Method | Endpoint |
|--------|----------|
| CRUD | `/api/v1/grades` |
| CRUD | `/api/v1/grades/{gradeId}/groups` |
| CRUD | `/api/v1/subjects` |
| CRUD | `/api/v1/rooms` |
| CRUD | `/api/v1/grades/{gradeId}/subject-requirements` |
| CRUD | `/api/v1/grades/{gradeId}/combined-lessons` |

### Time Structure

| Method | Endpoint |
|--------|----------|
| CRUD | `/api/v1/teaching-days` |
| CRUD | `/api/v1/teaching-days/{dayId}/time-slots` |
| CRUD | `/api/v1/grades/{gradeId}/day-config` |

### Teachers

| Method | Endpoint |
|--------|----------|
| CRUD | `/api/v1/teachers` |
| CRUD | `/api/v1/teachers/{id}/qualifications` |
| CRUD | `/api/v1/teachers/{id}/day-config` |
| CRUD | `/api/v1/teachers/{id}/blocked-slots` |

All resource endpoints (except auth) require the `ScheduleManagerOrAbove` policy (OrgAdmin or ScheduleManager role).

## Multi-Tenancy

Each school is a tenant. Registration creates a new tenant and an OrgAdmin user. All data is isolated per tenant using:

- A `TenantId` column on all tenant-owned tables
- EF Core global query filters applied automatically
- A `TenantInterceptor` that stamps `TenantId` on new entities
- The `tenant_id` JWT claim used to resolve the current tenant

## Roles

| Role | Permissions |
|------|-------------|
| **OrgAdmin** | Full access: manage school config, users, teachers, subjects, rooms, timetables |
| **ScheduleManager** | Create/edit timetables, view all config. Cannot manage users |
| **Viewer** | Read-only access to published timetables |

## License

All rights reserved.
