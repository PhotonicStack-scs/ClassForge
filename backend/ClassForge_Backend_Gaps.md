# ClassForge — Backend Gaps & Suggested Enhancements

These are API features or changes that the frontend spec requires but are not currently present in the backend API (based on review of `swagger.json` and `README.md`).

Prioritized by importance: **P0** = blocks core frontend functionality, **P1** = needed for good UX, **P2** = nice to have.

---

## P0 — Required for Core Frontend

### 1. Subject Color Field

**Entity:** Subject
**Change:** Add a `color` field (string, hex color code, e.g. "#DBEAFE") to the Subject entity.
**Endpoints affected:** `CreateSubjectRequest`, `UpdateSubjectRequest`, `SubjectResponse`
**Reason:** The frontend color-codes timetable cells by subject. Colors must persist in the database so they're consistent across users and sessions. Auto-assign a default from the subject palette (20 colors) on creation.

### 2. Tenant Default Language

**Entity:** Tenant
**Change:** Add a `defaultLanguage` field (string, e.g. "nb", "nn", "en") to the Tenant entity.
**Endpoints affected:** `UpdateTenantRequest`, `TenantResponse`
**Default:** "nb"
**Reason:** The frontend uses this to determine the default UI language for the school. Individual users can override in their profile.

### 3. User Language Preference

**Entity:** User
**Change:** Add a `languagePreference` field (string, nullable, e.g. "nb", "nn", "en") to the User entity.
**Endpoints affected:** `CreateUserRequest`, `UpdateUserRequest`, `UserResponse`, `UserProfileResponse`
**Reason:** Users can override the tenant's default language. If null, falls back to tenant default.

### 4. Timetable Generation Progress

**Current state:** The `TimetableResponse` has a `status` field (Generating/Draft/Published/etc.) and `errorMessage`.
**Change:** Add a `progressPercentage` field (int?, 0–100) to `TimetableResponse`. The scheduling engine should update this periodically during generation.
**Reason:** The frontend polls for status during generation and wants to show a progress bar. Without percentage, we can only show a spinner.

### 5. Teacher-User Linking for "My Schedule"

**Current state:** Teachers have an optional `email` field. Users have an `email` field. There's no explicit link.
**Change:** Add an endpoint to resolve the current user's teacher record:
  - `GET /api/v1/auth/my-teacher` → returns the `TeacherResponse` for the teacher whose email matches the logged-in user's email, or 404 if no match.
**Reason:** The "My Schedule" page for Viewer-role users needs to know which teacher they are to fetch their timetable view. Matching by email works, but having an explicit endpoint is cleaner and allows the backend to handle edge cases (multiple matches, case sensitivity).

### 6. Published Timetable Shortcut

**Change:** Add an endpoint:
  - `GET /api/v1/timetables/published` → returns the most recently published timetable (or 404 if none).
**Reason:** The "My Schedule" page and Viewer-role users need to access the currently active timetable without knowing its ID. Avoids the client having to list all timetables and filter.

---

## P1 — Needed for Good UX

### 7. Setup Wizard Completion State

**Change:** Add a `setupCompleted` field (bool) and optionally `setupProgress` (JSON or structured object tracking which steps are done) to the Tenant entity.
**Endpoints affected:** `TenantResponse`, plus a new `PUT /api/v1/school/setup-progress` endpoint.
**Reason:** The frontend needs to know whether to show the setup wizard on login, and which steps have been completed, so users can resume where they left off.

### 8. Bulk Operations

**Change:** Add bulk endpoints:
  - `POST /api/v1/grades/bulk` — create multiple grades at once (for wizard template population)
  - `POST /api/v1/subjects/bulk` — create multiple subjects at once
  - `POST /api/v1/teachers/bulk` — create multiple teachers at once (CSV import)
  - `POST /api/v1/grades/{gradeId}/subject-requirements/bulk` — set the entire curriculum matrix for a grade in one call
  - `POST /api/v1/teaching-days/{dayId}/time-slots/bulk` — create all time slots for a day in one call
**Reason:** The setup wizard populates a template that can create 7+ grades, 10+ subjects, 30+ time slots, and a full curriculum matrix. Doing this one-by-one would require dozens of API calls and be very slow. Bulk endpoints are essential for a smooth wizard experience.

### 9. Day Name in TeachingDayResponse

**Current state:** `TeachingDayResponse` only has `dayOfWeek` (int), `isActive`, `sortOrder`.
**Change:** Add a `name` field (string) that returns the localized day name, or at minimum return the standard English name ("Monday", "Tuesday"...). Alternatively, the frontend can derive it from `dayOfWeek`, but having it in the response is more convenient.
**Lower priority** since the frontend can map day numbers to names using its own locale data.

### 10. Subject Name in Qualification/Requirement Responses

**Current state:** `TeacherQualificationResponse` only returns `subjectId`, `minGradeId`, `maxGradeId` (UUIDs). Same for `GradeSubjectRequirementResponse` which only returns `subjectId`.
**Change:** Add `subjectName`, `minGradeName`, `maxGradeName` to `TeacherQualificationResponse`. Add `subjectName` to `GradeSubjectRequirementResponse`.
**Reason:** The frontend would otherwise need to cross-reference every UUID against the subjects/grades lists for every displayed qualification and requirement. Denormalized names in the response avoid N+1 display issues and simplify the frontend code significantly.

### 11. Dashboard Statistics Endpoint

**Change:** Add `GET /api/v1/school/stats` returning:
```json
{
  "gradeCount": 10,
  "groupCount": 30,
  "teacherCount": 45,
  "subjectCount": 15,
  "roomCount": 3,
  "timetableCount": 2,
  "publishedTimetableId": "uuid-or-null"
}
```
**Reason:** The dashboard needs quick stats without making 6+ separate list API calls and counting results.

---

## P2 — Nice to Have

### 12. CSV Teacher Import Endpoint

**Change:** Add `POST /api/v1/teachers/import` accepting a CSV file (multipart/form-data) with columns: name, email.
**Reason:** Bulk teacher creation from a file is a common school admin workflow. The frontend can parse CSV client-side and use the bulk endpoint (P1 #8) as an alternative, but a dedicated import endpoint with server-side validation and error reporting per row would be more robust.

### 13. Timetable Comparison

**Change:** Add `GET /api/v1/timetables/compare?ids={id1},{id2}` returning side-by-side quality scores and report differences.
**Reason:** Users may generate multiple timetables to compare quality. A comparison endpoint avoids fetching two full timetables and computing differences client-side.

### 14. Timetable Duplication

**Change:** Add `POST /api/v1/timetables/{id}/duplicate` that creates a new Draft timetable with the same entries.
**Reason:** Users may want to duplicate a timetable to experiment with manual edits without affecting the original.

### 15. Audit Log

**Change:** Track who changed what and when. `GET /api/v1/audit-log?entityType=Timetable&entityId={id}` returning a list of changes.
**Reason:** Multi-user SaaS accountability. Lower priority for MVP.

---

## Summary

| # | Feature | Priority | Effort Estimate |
|---|---------|----------|-----------------|
| 1 | Subject color field | P0 | Small — add field + migration |
| 2 | Tenant default language | P0 | Small — add field + migration |
| 3 | User language preference | P0 | Small — add field + migration |
| 4 | Generation progress percentage | P0 | Medium — requires scheduler changes |
| 5 | My-teacher resolution endpoint | P0 | Small — new endpoint |
| 6 | Published timetable shortcut | P0 | Small — new endpoint |
| 7 | Setup wizard state | P1 | Small — add fields + endpoint |
| 8 | Bulk creation endpoints | P1 | Medium — 5 new endpoints with validation |
| 9 | Day name in response | P1 | Trivial — add computed field |
| 10 | Denormalized names in responses | P1 | Small — modify response DTOs + queries |
| 11 | Dashboard stats endpoint | P1 | Small — new endpoint |
| 12 | CSV teacher import | P2 | Medium — file parsing + validation |
| 13 | Timetable comparison | P2 | Medium — new endpoint with diff logic |
| 14 | Timetable duplication | P2 | Small — new endpoint |
| 15 | Audit log | P2 | Large — cross-cutting concern |

I recommend implementing all P0 items and P1 items #8, #10, #11 before starting frontend development. The remaining P1 and P2 items can be added iteratively as the frontend matures.
