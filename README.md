# ClassForge

A multi-tenant SaaS backend that automatically generates weekly class timetables for schools. Schools define their structure (grades, groups, rooms), teachers (availability, qualifications), and curriculum requirements, then ClassForge uses a constraint-satisfaction algorithm to produce optimized weekly schedules.

## Tech Stack

- **Runtime:** .NET 10 (C#)
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core
- **API:** ASP.NET Core Minimal APIs
- **Auth:** JWT Bearer + OAuth2 (Google, Microsoft)
- **Scheduling:** Custom CSP solver (constraint propagation + backtracking with MRV/LCV)
- **Async Processing:** BackgroundService + Channel&lt;T&gt;
- **Logging:** Serilog
- **Validation:** FluentValidation
- **Testing:** xUnit v3, FluentAssertions, NSubstitute, Testcontainers (PostgreSQL)
- **Architecture:** Clean Architecture

## Prerequisites

- [.NET SDK 10.0.101](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (latest stable)
- [Docker](https://www.docker.com/) (for integration tests with Testcontainers)

## Getting Started

### 1. Clone and restore

```bash
git clone <repo-url>
cd ClassForge
dotnet restore ClassForge.sln
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

Browse to `/swagger` for interactive API documentation with typed request/response schemas for all endpoints.

In Development mode, a demo school is automatically seeded with 5 grades, 15 groups, 15 subjects, 30 teachers, and a full weekly time structure.

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
  ClassForge.Infrastructure/    EF Core DbContext, configurations, services, async generation.
  ClassForge.Scheduling/        Timetable generation engine (CSP solver). Depends on Application only.
  ClassForge.API/               Program.cs, endpoints, middleware, filters.

tests/
  ClassForge.Tests.Unit/        Unit tests for scheduling algorithm and constraints.
  ClassForge.Tests.Integration/ Integration tests with Testcontainers PostgreSQL.
```

Dependency flow: Domain &larr; Application &larr; Infrastructure / Scheduling &larr; API

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

### Timetables

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/timetables/preflight` | Run pre-flight validation on all config |
| GET | `/api/v1/timetables` | List all timetables |
| POST | `/api/v1/timetables` | Create timetable + start async generation (returns 202) |
| GET | `/api/v1/timetables/{id}` | Get timetable with status (poll for generation progress) |
| PUT | `/api/v1/timetables/{id}` | Update timetable name |
| DELETE | `/api/v1/timetables/{id}` | Delete timetable with all entries |
| GET | `/api/v1/timetables/{id}/entries` | Get entries (filter by ?groupId, ?teacherId, ?teachingDayId) |
| PUT | `/api/v1/timetables/{id}/entries/{entryId}` | Edit a single entry (Draft only, validates constraints) |
| GET | `/api/v1/timetables/{id}/report` | Get quality report |
| POST | `/api/v1/timetables/{id}/publish` | Change status Draft &rarr; Published |
| POST | `/api/v1/timetables/{id}/validate` | Re-validate all entries against hard constraints |
| GET | `/api/v1/timetables/{id}/by-group/{groupId}` | Weekly view for a group |
| GET | `/api/v1/timetables/{id}/by-teacher/{teacherId}` | Weekly view for a teacher |
| GET | `/api/v1/timetables/{id}/by-room/{roomId}` | Weekly view for a room |

All resource endpoints (except auth) require the `ScheduleManagerOrAbove` policy (OrgAdmin or ScheduleManager role).

## Scheduling Engine

The timetable generator uses a **Constraint Satisfaction Problem (CSP)** approach:

1. **Constraint Propagation** -- Builds lesson variables from requirements, computes initial domains of feasible (teacher, timeSlot, room) assignments, and runs arc consistency to prune impossible values.

2. **Backtracking Search** -- Picks the variable with the smallest domain (MRV heuristic), orders values by fewest eliminations (LCV) and soft constraint score, then recursively assigns with forward checking.

3. **Hard Constraints (HC-1 through HC-10):**
   - No teacher/group/room double-booking
   - Teacher blocked slots and daily hour limits respected
   - All required periods scheduled
   - Special rooms assigned when required
   - Grade day config limits respected
   - Double periods use consecutive non-break slots
   - Subject daily period limits respected

4. **Soft Constraints (weighted scoring):**
   - Same teacher per subject per group (1000)
   - Minimize teacher schedule gaps (100)
   - Avoid same subject twice in a day (50)
   - Even distribution across the week (20)
   - Honor double period preferences (10)
   - Utilize combined lessons (5)

5. **Post-solve Reporting** -- Detects teacher splits, schedule gaps, subject clustering, unused double periods and combined lessons, and infeasible assignments.

### Pre-flight Validation

Before generating, `POST /preflight` checks:
- Subjects with no qualified teachers
- Teachers with zero available hours
- Grades exceeding available time slots
- Combined lesson groups not in their grade
- Invalid subject references
- Active teaching days with no non-break slots
- Room capacity insufficient for combined lessons

### Manual Editing

After generation, Draft timetables can be manually edited entry-by-entry via `PUT /entries/{entryId}`. Each edit is validated against all hard constraints; violations return `409 Conflict` with a list of issues.

## Multi-Tenancy

Each school is a tenant. Registration creates a new tenant and an OrgAdmin user. All data is isolated per tenant using:

- A `TenantId` column on all tenant-owned tables
- EF Core global query filters applied automatically
- A `TenantInterceptor` that stamps `TenantId` on new entities
- The `tenant_id` JWT claim used to resolve the current tenant
- `TenantProvider.SetTenantId()` override for background services without HTTP context

## Roles

| Role | Permissions |
|------|-------------|
| **OrgAdmin** | Full access: manage school config, users, teachers, subjects, rooms, timetables |
| **ScheduleManager** | Create/edit timetables, view all config. Cannot manage users |
| **Viewer** | Read-only access to published timetables |

## Testing

```bash
# Run all tests
dotnet test

# Unit tests only (no Docker required)
dotnet test tests/ClassForge.Tests.Unit

# Integration tests (requires Docker for Testcontainers PostgreSQL)
dotnet test tests/ClassForge.Tests.Integration
```

**Unit tests** cover the scheduling algorithm: hard constraints, soft constraint scoring, constraint propagation/domain reduction, and backtracking solver (trivial solve, multi-group, combined lessons, infeasibility detection).

**Integration tests** cover API endpoints with a real PostgreSQL database via Testcontainers: authentication flow, CRUD operations, tenant isolation, timetable lifecycle.

## License

All rights reserved.
