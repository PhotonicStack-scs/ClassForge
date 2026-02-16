# ClassForge — Backend Requirements Specification

**C# .NET 10 | PostgreSQL | REST API**

Version 1.0 — February 2026
*Prepared for development with Claude Code*

---

## 1. Project Overview

The ClassForge is a multi-tenant SaaS application that automatically generates weekly class timetables for schools. It takes as input the school's structure (grades, groups, rooms), teachers (availability, qualifications), and curriculum requirements (subject hours per grade), then uses a constraint-satisfaction algorithm to produce optimized weekly schedules.

### 1.1 Technology Stack

- **Runtime:** C# .NET 10
- **Database:** PostgreSQL (latest stable)
- **ORM:** Entity Framework Core
- **API:** RESTful (ASP.NET Core Minimal APIs or Controllers)
- **Authentication:** ASP.NET Core Identity + OAuth2 (Google, Microsoft)
- **Architecture:** Clean Architecture (Domain → Application → Infrastructure → API)

### 1.2 Multi-Tenancy

One school equals one tenant. All data is tenant-scoped. Use a `TenantId` column on all tenant-owned tables with a global query filter in EF Core to enforce isolation. Each tenant (school) has its own configuration for grades, time structure, teachers, and subjects.

### 1.3 Authentication & Authorization

Support two authentication methods from launch:

- **Email/password:** Using ASP.NET Core Identity with bcrypt/PBKDF2 password hashing.
- **OAuth2/OIDC:** Google and Microsoft identity providers.

Three roles, scoped per tenant:

| Role | Permissions |
|------|-------------|
| Org Admin | Full access. Manage school config, users, teachers, subjects, rooms, timetables. |
| Schedule Manager | Create, edit, and generate timetables. View all config. Cannot manage users. |
| Viewer | Read-only. View published timetables (teachers view their own schedule). |

---

## 2. Data Model

Below is the complete set of domain entities. All tenant-owned entities include a `TenantId` foreign key. Use UUID/GUID for all primary keys.

### 2.1 Tenant / School

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| Name | string | School name |
| CreatedAt | datetime | Creation timestamp |

### 2.2 User

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TenantId | UUID (FK) | School this user belongs to |
| Email | string | Unique per tenant |
| PasswordHash | string? | Null for OAuth-only users |
| ExternalProvider | string? | e.g. "Google", "Microsoft" |
| ExternalId | string? | Provider-specific user ID |
| Role | enum | OrgAdmin \| ScheduleManager \| Viewer |
| DisplayName | string | Full name for display |

### 2.3 Grade

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TenantId | UUID (FK) | School |
| Name | string | Display name, e.g. "5th Grade" |
| SortOrder | int | For ordering in UI |

### 2.4 Group

A subdivision of a grade. E.g. Grade 5 may have groups 5A, 5B, 5C.

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TenantId | UUID (FK) | School |
| GradeId | UUID (FK) | Parent grade |
| Name | string | e.g. "A", "B", "C" |
| SortOrder | int | For ordering |

### 2.5 Subject

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TenantId | UUID (FK) | School |
| Name | string | e.g. "Mathematics" |
| RequiresSpecialRoom | bool | If true, must be scheduled in an assigned room |
| SpecialRoomId | UUID? (FK) | The required room (nullable, required if RequiresSpecialRoom) |
| MaxPeriodsPerDay | int | Default 2. Max consecutive/daily slots for this subject per group |
| AllowDoublePeriods | bool | Whether this subject can be scheduled as a double period |

### 2.6 Room

Only special-purpose rooms need to be defined (e.g. science lab, gym, music room). Regular classrooms are implicit per group.

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TenantId | UUID (FK) | School |
| Name | string | e.g. "Science Lab 1" |
| Capacity | int | Max number of groups that can use this room simultaneously (for combined lessons) |

### 2.7 Grade Subject Requirement

Defines how many periods per week each grade needs for each subject. Uniform across all groups in the same grade.

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TenantId | UUID (FK) | School |
| GradeId | UUID (FK) | The grade |
| SubjectId | UUID (FK) | The subject |
| PeriodsPerWeek | int | Required number of periods per week |
| PreferDoublePeriods | bool | Hint to scheduler to prefer double periods for this subject |

### 2.8 Combined Lesson Config

Defines which groups within a grade may be combined for a particular subject. Combining only happens within the same grade.

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TenantId | UUID (FK) | School |
| GradeId | UUID (FK) | Grade |
| SubjectId | UUID (FK) | Subject |
| IsMandatory | bool | If true, these groups MUST be combined; if false, the scheduler MAY combine them |
| MaxGroupsPerLesson | int | Max groups combined into one lesson (e.g. 2) |

Additionally, a junction table **CombinedLessonGroup** stores which specific groups are eligible for combining:

| Field | Type | Description |
|-------|------|-------------|
| CombinedLessonConfigId | UUID (FK) | Parent config |
| GroupId | UUID (FK) | Eligible group |

### 2.9 Teacher

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TenantId | UUID (FK) | School |
| Name | string | Full name |
| Email | string? | Optional, for linking to a Viewer account |

### 2.10 Teacher Subject Qualification

Defines which subjects a teacher can teach, and at which grade levels.

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TeacherId | UUID (FK) | The teacher |
| SubjectId | UUID (FK) | The subject they can teach |
| MinGradeId | UUID (FK) | Lowest grade they teach this subject |
| MaxGradeId | UUID (FK) | Highest grade they teach this subject |

### 2.11 Time Structure

The weekly time structure is defined per tenant.

**TeachingDay:** Defines which days of the week are active teaching days.

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TenantId | UUID (FK) | School |
| DayOfWeek | int | 0=Monday, 1=Tuesday, ... 6=Sunday |
| IsActive | bool | Whether this is a teaching day |
| SortOrder | int | Display order |

**TimeSlot:** Defines the period structure for each active day.

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TenantId | UUID (FK) | School |
| TeachingDayId | UUID (FK) | Which day this slot belongs to |
| SlotNumber | int | Order within the day (1, 2, 3...) |
| StartTime | TimeOnly | e.g. 08:00 |
| EndTime | TimeOnly | e.g. 08:45 |
| IsBreak | bool | If true, this is a break/lunch slot (not schedulable) |

> **Note:** The period duration should be uniform (e.g. all 45 min). Breaks are modeled as time slots with `IsBreak=true`. Two consecutive non-break slots with no break slot between them are eligible for double periods.

### 2.12 Grade Day Config

Defines how many periods a grade/group has per day. This allows different grades to finish at different times.

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TenantId | UUID (FK) | School |
| GradeId | UUID (FK) | The grade |
| TeachingDayId | UUID (FK) | Which day |
| MaxPeriods | int | Max periods for this grade on this day |

### 2.13 Teacher Availability

**TeacherDayConfig:** Sets work hours per teacher per day.

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TeacherId | UUID (FK) | The teacher |
| TeachingDayId | UUID (FK) | Which day |
| MaxPeriods | int | Max teaching periods on this day (0 = not working) |

**TeacherSlotBlock:** Blocks a specific time slot for a teacher (recurring weekly).

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TeacherId | UUID (FK) | The teacher |
| TimeSlotId | UUID (FK) | The blocked slot |
| Reason | string? | Optional note (e.g. "Staff meeting") |

### 2.14 Timetable (Output)

**Timetable:** A generated timetable for a semester.

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TenantId | UUID (FK) | School |
| Name | string | e.g. "Fall 2026" |
| Status | enum | Draft \| Published \| Archived |
| GeneratedAt | datetime | When the algorithm ran |
| QualityScore | decimal? | Overall quality metric (0-100) |
| CreatedBy | UUID (FK) | User who triggered generation |

**TimetableEntry:** A single scheduled lesson.

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TimetableId | UUID (FK) | Parent timetable |
| TimeSlotId | UUID (FK) | The period |
| SubjectId | UUID (FK) | The subject |
| TeacherId | UUID (FK) | Assigned teacher |
| RoomId | UUID? (FK) | Special room (null = home classroom) |
| IsDoublePeriod | bool | If true, also occupies the next consecutive slot |
| CombinedLessonGroupId | UUID? | If part of a combined lesson, links entries together |

**TimetableEntryGroup:** Junction table linking entries to groups (supports combined lessons).

| Field | Type | Description |
|-------|------|-------------|
| TimetableEntryId | UUID (FK) | The entry |
| GroupId | UUID (FK) | The group attending this lesson |

**TimetableReport:** Quality report generated alongside the timetable.

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary key |
| TimetableId | UUID (FK) | Parent timetable |
| Type | enum | Info \| Warning \| Error |
| Category | string | e.g. "TeacherSplit", "GapInSchedule", "UnmetHours" |
| Message | string | Human-readable description |
| RelatedEntityType | string? | e.g. "Teacher", "Group", "Subject" |
| RelatedEntityId | UUID? | ID of the related entity |

---

## 3. Scheduling Algorithm

The timetable generation is a constraint satisfaction / optimization problem. The algorithm must satisfy all hard constraints and optimize for soft constraints with the priority order defined below.

### 3.1 Hard Constraints (Must Never Be Violated)

If any hard constraint cannot be satisfied, the algorithm must report the conflict rather than produce an invalid timetable.

- **HC-1:** No teacher may be assigned to more than one lesson at the same time slot.
- **HC-2:** No group may have more than one lesson at the same time slot.
- **HC-3:** No special room may be double-booked beyond its capacity.
- **HC-4:** Teachers may only be scheduled during slots where they are available (respecting TeacherDayConfig and TeacherSlotBlock).
- **HC-5:** Teacher work hours per day must not be exceeded.
- **HC-6:** All required subject hours per week per grade must be fully scheduled.
- **HC-7:** A subject marked as requiring a special room must be scheduled in that room.
- **HC-8:** Groups may only be scheduled during slots allowed by their GradeDayConfig.
- **HC-9:** Double periods must only use consecutive non-break slots.
- **HC-10:** MaxPeriodsPerDay per subject must be respected.

### 3.2 Soft Constraints (Optimization Priorities)

Listed in descending priority. The algorithm should satisfy higher-priority soft constraints first.

- **SC-1:** Single teacher per subject per group: Each group should ideally have the same teacher for all weekly periods of a given subject. This is the strongest optimization preference.
- **SC-2:** Minimize teacher schedule gaps: Avoid free periods sandwiched between teaching periods in a teacher's daily schedule.
- **SC-3:** Avoid same subject appearing twice in a day for a group unless it is a double period.
- **SC-4:** Even distribution: Spread a subject's periods across the week rather than clustering on one or two days.
- **SC-5:** Respect double period preferences: When a GradeSubjectRequirement has `PreferDoublePeriods=true`, prefer scheduling double periods.
- **SC-6:** Utilize combined lessons: When configured, prefer combining groups where it satisfies room and teacher constraints.

### 3.3 Algorithm Approach

The recommended approach is a two-phase algorithm:

**Phase 1 — Constraint Propagation:** Use an approach similar to AC-3 (arc consistency) to reduce the domain of possible assignments. For each (group, subject, periodsNeeded) tuple, determine which (teacher, timeSlot) pairs are feasible given the hard constraints. Prune impossible assignments early.

**Phase 2 — Backtracking Search with Heuristics:** Use a backtracking search to assign lessons to slots. Apply the following heuristics:

- **Most Constrained Variable (MRV):** Schedule the group-subject combination with the fewest valid options first.
- **Least Constraining Value (LCV):** When choosing a teacher/slot, prefer the assignment that leaves the most options open for future assignments.
- **Soft constraint scoring:** Evaluate candidate assignments against the soft constraint priority list and prefer assignments with higher scores.

**Phase 3 (optional) — Local Search Improvement:** After finding a valid solution, apply local search (e.g. simulated annealing or hill climbing with random restarts) to improve the soft constraint score by swapping assignments.

> **Implementation note:** For the initial version, a well-implemented Phase 1 + Phase 2 should be sufficient for most schools (up to ~500 students, 50 teachers). Phase 3 can be added later if optimization quality needs improvement.

### 3.4 Output & Reporting

After generation, the algorithm produces:

- The complete `TimetableEntry` set representing the weekly schedule.
- A `TimetableReport` with entries categorized as Info, Warning, or Error.

Report categories should include:

| Category | Severity | Description |
|----------|----------|-------------|
| TeacherSplit | Warning | A subject for a group required more than one teacher |
| GapInTeacherSchedule | Info | A teacher has a gap between lessons on a given day |
| SubjectClustering | Info | A subject appears on consecutive days instead of being spread out |
| DoublePeriodNotUsed | Info | A double period was preferred but could not be scheduled |
| CombinedLessonNotUsed | Info | A combined lesson was configured but could not be scheduled |
| InfeasibleConstraint | Error | A hard constraint could not be satisfied (generation failed) |

---

## 4. REST API Design

All endpoints are tenant-scoped. The tenant is derived from the authenticated user's JWT token. Use standard HTTP methods and status codes. All requests/responses use JSON.

Base URL pattern: `/api/v1/{resource}`

### 4.1 Authentication

| Endpoint | Method | Description |
|----------|--------|-------------|
| /api/v1/auth/register | POST | Register with email/password |
| /api/v1/auth/login | POST | Login with email/password, returns JWT |
| /api/v1/auth/oauth/{provider} | GET | Initiate OAuth flow (google, microsoft) |
| /api/v1/auth/oauth/{provider}/callback | GET | OAuth callback, returns JWT |
| /api/v1/auth/refresh | POST | Refresh JWT token |
| /api/v1/auth/me | GET | Get current user profile |

### 4.2 School Configuration

| Endpoint | Method | Description |
|----------|--------|-------------|
| /api/v1/school | GET/PUT | Get or update school (tenant) details |
| /api/v1/users | CRUD | Manage users within the tenant (Org Admin only) |

### 4.3 Academic Structure

| Endpoint | Method | Description |
|----------|--------|-------------|
| /api/v1/grades | CRUD | Manage grades |
| /api/v1/grades/{gradeId}/groups | CRUD | Manage groups within a grade |
| /api/v1/subjects | CRUD | Manage subjects (incl. room requirements, double period config) |
| /api/v1/rooms | CRUD | Manage special rooms |
| /api/v1/grades/{gradeId}/subject-requirements | CRUD | Set periods-per-week for each subject at each grade |
| /api/v1/grades/{gradeId}/combined-lessons | CRUD | Configure combined lesson rules for a grade |

### 4.4 Time Structure

| Endpoint | Method | Description |
|----------|--------|-------------|
| /api/v1/teaching-days | CRUD | Manage active teaching days |
| /api/v1/teaching-days/{dayId}/time-slots | CRUD | Manage time slots/periods per day |
| /api/v1/grades/{gradeId}/day-config | CRUD | Set max periods per grade per day |

### 4.5 Teachers

| Endpoint | Method | Description |
|----------|--------|-------------|
| /api/v1/teachers | CRUD | Manage teachers |
| /api/v1/teachers/{id}/qualifications | CRUD | Manage subject qualifications & grade ranges |
| /api/v1/teachers/{id}/day-config | CRUD | Set work hours per day |
| /api/v1/teachers/{id}/blocked-slots | CRUD | Manage recurring slot blocks |

### 4.6 Timetable Generation

| Endpoint | Method | Description |
|----------|--------|-------------|
| /api/v1/timetables | GET/POST | List timetables or trigger new generation |
| /api/v1/timetables/{id} | GET/PUT/DELETE | Get, update status, or delete a timetable |
| /api/v1/timetables/{id}/entries | GET | Get all entries (filterable by group, teacher, day) |
| /api/v1/timetables/{id}/entries/{entryId} | PUT | Manual edit of a single entry (for tweaking) |
| /api/v1/timetables/{id}/report | GET | Get quality report |
| /api/v1/timetables/{id}/publish | POST | Change status to Published |
| /api/v1/timetables/{id}/validate | POST | Re-validate current entries against hard constraints |

### 4.7 Views (Convenience Endpoints)

| Endpoint | Method | Description |
|----------|--------|-------------|
| /api/v1/timetables/{id}/by-group/{groupId} | GET | Weekly timetable for a specific group |
| /api/v1/timetables/{id}/by-teacher/{teacherId} | GET | Weekly timetable for a specific teacher |
| /api/v1/timetables/{id}/by-room/{roomId} | GET | Weekly schedule for a specific room |

---

## 5. Non-Functional Requirements

### 5.1 Performance

- Timetable generation for a school with up to 30 grades, 3 groups per grade, 50 teachers, and 15 subjects should complete within 60 seconds.
- API response time for CRUD operations: < 200ms (p95).
- Timetable generation should run asynchronously. The POST endpoint returns immediately with a timetable ID and "Generating" status. The client polls for completion.

### 5.2 Data Validation

- All inputs must be validated at the API layer (FluentValidation or similar).
- Before timetable generation, run a pre-flight validation that checks for obvious issues: subjects with no qualified teachers, teachers with zero available hours, grades with more required periods than available slots.
- Return clear, actionable error messages.

### 5.3 Database

- Use EF Core migrations for schema management.
- Add indexes on TenantId + commonly filtered columns.
- Use row-level security or global query filters for tenant isolation.
- Support soft-delete for teachers and timetables.

### 5.4 Logging & Observability

- Structured logging (Serilog or similar) with correlation IDs.
- Log timetable generation progress (percentage complete) for long-running generations.
- Health check endpoint at `/health`.

### 5.5 Testing

- Unit tests for the scheduling algorithm (constraint validation, basic scheduling scenarios).
- Integration tests for API endpoints with an in-memory or test database.
- Seed data scripts for development/demo purposes.

---

## 6. Recommended Project Structure

Follow Clean Architecture with the following projects in the solution:

| Project | Layer | Contents |
|---------|-------|----------|
| ClassForge.Domain | Domain | Entities, enums, value objects, domain events. No dependencies. |
| ClassForge.Application | Application | Use cases, interfaces (IRepository, ITimetableGenerator), DTOs, validators. |
| ClassForge.Infrastructure | Infrastructure | EF Core DbContext, repositories, auth providers, external integrations. |
| ClassForge.Scheduling | Infrastructure | The scheduling algorithm implementation. Depends on Domain and Application interfaces. |
| ClassForge.API | Presentation | ASP.NET Core API, controllers/endpoints, middleware, DI configuration. |
| ClassForge.Tests.Unit | Tests | Unit tests for scheduling algorithm and domain logic. |
| ClassForge.Tests.Integration | Tests | Integration tests for API and database. |

---

## 7. Implementation Order (Suggested Milestones)

Recommended order for incremental development:

1. **Project scaffolding:** Solution structure, EF Core setup, PostgreSQL connection, migrations, health check.
2. **Authentication:** Identity setup, email/password registration and login, JWT issuance, OAuth2 flows.
3. **Tenant & user management:** Tenant CRUD, user CRUD with roles, tenant isolation via global query filter.
4. **Academic structure:** Grades, groups, subjects, rooms, subject requirements CRUD.
5. **Time structure:** Teaching days, time slots, grade day config, break definitions.
6. **Teacher management:** Teachers, qualifications, day config, blocked slots.
7. **Combined lesson config:** Setup for group merging per subject per grade.
8. **Pre-flight validation:** Validate that all configuration is consistent before generation.
9. **Scheduling algorithm (core):** Implement Phase 1 (constraint propagation) + Phase 2 (backtracking search).
10. **Timetable output & reporting:** Generate entries, quality report, convenience view endpoints.
11. **Manual editing:** Allow tweaking individual entries post-generation with re-validation.
12. **Seed data & testing:** Comprehensive test suite + demo seed data.

---

## 8. Glossary

| Term | Definition |
|------|------------|
| Grade | A year/level in the school (e.g. 5th grade). Contains one or more groups. |
| Group | A subdivision of a grade (e.g. 5A, 5B). The atomic unit that attends lessons together. |
| Period / Time Slot | A fixed-length teaching block (e.g. 45 minutes). The smallest schedulable unit. |
| Double Period | Two consecutive non-break periods used for one subject. Counted as 2 periods for weekly requirements. |
| Combined Lesson | A lesson where multiple groups from the same grade attend together (e.g. PE for 5A + 5B). |
| Teaching Day | A day of the week when classes are held (default: Mon-Fri, configurable). |
| Break | A non-schedulable time slot between periods (short break or lunch). |
| Special Room | A room required by certain subjects (e.g. lab, gym). Must be explicitly booked. |
| Home Classroom | The default room for a group. Not modeled explicitly; assumed available. |
| Qualification | A teacher's ability to teach a specific subject at specific grade levels. |
| Tenant | A school. All data is isolated per tenant in the multi-tenant architecture. |
