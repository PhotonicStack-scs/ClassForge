# ClassForge Backend — Terminology Refactoring Plan

## Overview

Rename four domain terms throughout the entire backend codebase:

| Old Term | New Term | Norwegian | Reason |
|----------|----------|-----------|--------|
| Grade | Year | Trinn/Klassetrinn | "Grade" is ambiguous with student scores (karakter) |
| Group | Class | Klasse | "Class" is the natural translation of klasse |
| Teaching Day | School Day | Skoledag | More natural in everyday language |
| Subject Requirement (GradeSubjectRequirement) | Curriculum (YearCurriculum) | Fag- og timefordeling | Domain-accurate |

**IMPORTANT:** This is a breaking API change. The frontend will be refactored separately to match. Complete the entire backend refactoring before moving to the frontend.

## Execution Order

Work through these steps in exact order. After each step, ensure `dotnet build ClassForge.sln` compiles successfully before moving to the next step. Run `dotnet test` after all steps are complete.

---

## Step 1: Domain Layer — Rename Entities & Properties

### 1.1 Rename entity classes (files and class names)

| Old File/Class | New File/Class |
|----------------|----------------|
| `Grade.cs` → | `Year.cs` / `Year` |
| `Group.cs` → | `Class.cs` / `Class` |
| `TeachingDay.cs` → | `SchoolDay.cs` / `SchoolDay` |
| `GradeSubjectRequirement.cs` → | `YearCurriculum.cs` / `YearCurriculum` |
| `GradeDayConfig.cs` → | `YearDayConfig.cs` / `YearDayConfig` |
| `CombinedLessonConfig.cs` → | (keep name, update properties) |
| `CombinedLessonGroup.cs` → | `CombinedLessonClass.cs` / `CombinedLessonClass` |
| `TimetableEntryGroup.cs` → | `TimetableEntryClass.cs` / `TimetableEntryClass` |

### 1.2 Rename properties on ALL entities that reference the old terms

**On the renamed entities themselves:**
- `Grade.Id` → `Year.Id` (automatic with class rename)
- `Group.GradeId` → `Class.YearId`
- `GradeDayConfig.GradeId` → `YearDayConfig.YearId`
- `GradeDayConfig.TeachingDayId` → `YearDayConfig.SchoolDayId`
- `GradeSubjectRequirement.GradeId` → `YearCurriculum.YearId`

**On other entities that reference them:**
- `TimeSlot.TeachingDayId` → `TimeSlot.SchoolDayId`
- `TimeSlot.TeachingDay` (navigation) → `TimeSlot.SchoolDay`
- `TeacherDayConfig.TeachingDayId` → `TeacherDayConfig.SchoolDayId`
- `TeacherDayConfig.TeachingDay` (navigation) → `TeacherDayConfig.SchoolDay`
- `TeacherSlotBlock.TimeSlot.TeachingDayId` — (indirect, no change needed on TeacherSlotBlock itself)
- `TeacherSubjectQualification.MinGradeId` → `TeacherSubjectQualification.MinYearId`
- `TeacherSubjectQualification.MaxGradeId` → `TeacherSubjectQualification.MaxYearId`
- `TeacherSubjectQualification.MinGrade` (navigation) → `TeacherSubjectQualification.MinYear`
- `TeacherSubjectQualification.MaxGrade` (navigation) → `TeacherSubjectQualification.MaxYear`
- `CombinedLessonConfig.GradeId` → `CombinedLessonConfig.YearId`
- `CombinedLessonConfig.Grade` (navigation) → `CombinedLessonConfig.Year`
- `CombinedLessonConfig.Groups` (navigation collection) → `CombinedLessonConfig.Classes`
- `CombinedLessonGroup.GroupId` → `CombinedLessonClass.ClassId`
- `TimetableEntry.CombinedLessonGroupId` → `TimetableEntry.CombinedLessonClassId`
- `TimetableEntry.Groups` (navigation collection) → `TimetableEntry.Classes`
- `TimetableEntryGroup.GroupId` → `TimetableEntryClass.ClassId`

**Navigation collections on parent entities:**
- `Year` (was Grade): `Groups` → `Classes`, `SubjectRequirements` → `Curricula`, `DayConfigs` stays as `DayConfigs`, `CombinedLessonConfigs` stays
- `SchoolDay` (was TeachingDay): any navigation collections referencing `TeachingDay` rename accordingly

### 1.3 Rename interfaces if any reference Grade/Group/TeachingDay

Check `ITenantEntity` and other interfaces — these likely don't reference specific entity names, but verify.

---

## Step 2: Domain Layer — Rename Enums/Values (if applicable)

Check if any enum values reference "Grade" or "Group" (e.g., in `ReportType` or report categories). The `TimetableReport` entity has `RelatedEntityType` which may contain string values like `"Group"` or `"Grade"` — these need to be updated to `"Class"` and `"Year"`.

---

## Step 3: Application Layer — Rename DTOs

| Old DTO | New DTO |
|---------|---------|
| `CreateGradeRequest` | `CreateYearRequest` |
| `UpdateGradeRequest` | `UpdateYearRequest` |
| `GradeResponse` | `YearResponse` |
| `CreateGroupRequest` | `CreateClassRequest` |
| `UpdateGroupRequest` | `UpdateClassRequest` |
| `GroupResponse` | `ClassResponse` |
| `CreateTeachingDayRequest` | `CreateSchoolDayRequest` |
| `UpdateTeachingDayRequest` | `UpdateSchoolDayRequest` |
| `TeachingDayResponse` | `SchoolDayResponse` |
| `CreateGradeSubjectRequirementRequest` | `CreateYearCurriculumRequest` |
| `UpdateGradeSubjectRequirementRequest` | `UpdateYearCurriculumRequest` |
| `GradeSubjectRequirementResponse` | `YearCurriculumResponse` |
| `CreateGradeDayConfigRequest` | `CreateYearDayConfigRequest` |
| `UpdateGradeDayConfigRequest` | `UpdateYearDayConfigRequest` |
| `GradeDayConfigResponse` | `YearDayConfigResponse` |
| `CreateCombinedLessonConfigRequest` | (keep name, rename `groupIds` → `classIds`) |
| `UpdateCombinedLessonConfigRequest` | (keep name, rename `groupIds` → `classIds`) |
| `CombinedLessonConfigResponse` | (keep name, rename `gradeId` → `yearId`, `groupIds` → `classIds`) |
| `TeacherQualificationResponse` | (keep name, rename `minGradeId`/`maxGradeId` → `minYearId`/`maxYearId`) |
| `CreateTeacherQualificationRequest` | (keep name, rename `minGradeId`/`maxGradeId` → `minYearId`/`maxYearId`) |
| `UpdateTeacherQualificationRequest` | (keep name, rename `minGradeId`/`maxGradeId` → `minYearId`/`maxYearId`) |
| `TeacherDayConfigResponse` | (keep name, rename `teachingDayId` → `schoolDayId`) |
| `CreateTeacherDayConfigRequest` | (keep name, rename `teachingDayId` → `schoolDayId`) |
| `TimeSlotResponse` | (keep name, rename `teachingDayId` → `schoolDayId`) |
| `CreateTimeSlotRequest` | (keep name, rename `teachingDayId` if present → `schoolDayId`) |
| `TimetableEntryResponse` | (rename `groupIds` → `classIds`, `combinedLessonGroupId` → `combinedLessonClassId`) |
| `UpdateTimetableEntryRequest` | (rename `groupIds` → `classIds`) |
| `TimetableViewEntry` | (rename `groupNames` → `classNames`) |

Also rename the DTO folder names:
- `DTOs/Grades/` → `DTOs/Years/`
- `DTOs/Groups/` → `DTOs/Classes/`
- `DTOs/TeachingDays/` → `DTOs/SchoolDays/`
- `DTOs/GradeSubjectRequirements/` → `DTOs/Curricula/` (or `DTOs/YearCurriculum/`)
- `DTOs/GradeDayConfig/` → `DTOs/YearDayConfig/`

---

## Step 4: Application Layer — Rename Validators

Rename all FluentValidation validator classes and files to match new DTO names:
- `CreateGradeRequestValidator` → `CreateYearRequestValidator`
- `CreateGroupRequestValidator` → `CreateClassRequestValidator`
- `CreateTeachingDayRequestValidator` → `CreateSchoolDayRequestValidator`
- `CreateGradeSubjectRequirementRequestValidator` → `CreateYearCurriculumRequestValidator`
- etc. for all Update validators

Update the property names inside validators (e.g., `RuleFor(x => x.GradeId)` → `RuleFor(x => x.YearId)`).

---

## Step 5: Application Layer — Rename Mapping Extensions

In `MappingExtensions.cs` (or wherever mappings live):
- Rename all `.ToResponse()` and `.ToEntity()` methods to use the new type names
- Update all property mappings inside these methods

---

## Step 6: Application Layer — Rename Interfaces

- `IAppDbContext`: Rename DbSet properties (e.g., `DbSet<Grade> Grades` → `DbSet<Year> Years`, `DbSet<Group> Groups` → `DbSet<Class> Classes`, etc.)
- `ITimetableGenerator`: Check if any method signatures reference Grade/Group types
- `IPreflightValidator`: Same check
- `ITimetableEntryValidator`: Same check
- `ITimetableGenerationQueue`: Same check

---

## Step 7: Infrastructure Layer — Rename EF Configurations

| Old File/Class | New File/Class |
|----------------|----------------|
| `GradeConfiguration.cs` | `YearConfiguration.cs` / `YearConfiguration` |
| `GroupConfiguration.cs` | `ClassConfiguration.cs` / `ClassConfiguration` |
| `TeachingDayConfiguration.cs` | `SchoolDayConfiguration.cs` / `SchoolDayConfiguration` |
| `GradeSubjectRequirementConfiguration.cs` | `YearCurriculumConfiguration.cs` / `YearCurriculumConfiguration` |
| `GradeDayConfigConfiguration.cs` | `YearDayConfigConfiguration.cs` / `YearDayConfigConfiguration` |
| `CombinedLessonGroupConfiguration.cs` | `CombinedLessonClassConfiguration.cs` |
| `TimetableEntryGroupConfiguration.cs` | `TimetableEntryClassConfiguration.cs` |

**Inside each configuration:**
- Update `HasIndex` calls (e.g., `HasIndex(e => e.GradeId)` → `HasIndex(e => e.YearId)`)
- Update `HasOne`/`HasMany`/`WithMany` navigation references
- Update table names if explicitly set via `.ToTable("TableName")` — rename to match new entity names
- Update unique constraint names if explicitly named
- Update foreign key column names if explicitly set

---

## Step 8: Infrastructure Layer — Rename AppDbContext DbSets

In `AppDbContext.cs`:
```
DbSet<Grade> Grades → DbSet<Year> Years
DbSet<Group> Groups → DbSet<Class> Classes  
DbSet<TeachingDay> TeachingDays → DbSet<SchoolDay> SchoolDays
DbSet<GradeSubjectRequirement> GradeSubjectRequirements → DbSet<YearCurriculum> YearCurricula
DbSet<GradeDayConfig> GradeDayConfigs → DbSet<YearDayConfig> YearDayConfigs
DbSet<CombinedLessonGroup> CombinedLessonGroups → DbSet<CombinedLessonClass> CombinedLessonClasses
DbSet<TimetableEntryGroup> TimetableEntryGroups → DbSet<TimetableEntryClass> TimetableEntryClasses
```

Also update the global query filter code in `OnModelCreating` if it references these types by name.

---

## Step 9: Infrastructure Layer — Rename Services

Update all service classes that reference old terminology:
- `SchedulingInputBuilder` — likely builds scheduling input from Grades/Groups/TeachingDays. Rename all variable names and references.
- `PreflightValidator` — references Grade/Group/TeachingDay in validation logic and error messages.
- `TimetableEntryValidator` — references Group in validation.
- `TimetableGenerationService` — references old entity names.
- Any other services in `Infrastructure/Services/`.

**Important:** Update human-readable error messages and report strings to use new terminology (e.g., "Grade has no qualified teachers" → "Year has no qualified teachers").

---

## Step 10: Scheduling Layer — Rename Internal Types

The Scheduling project has its own internal types (`SchedulingInput`, `SchedulingRequirement`, `LessonVariable`, etc.). These likely reference Grade/Group:
- Rename any `GradeId`/`GroupId` properties to `YearId`/`ClassId`
- Rename any `TeachingDay` references to `SchoolDay`
- Update `ConstraintPropagation`, `BacktrackingSolver`, `HardConstraintChecker`, `SoftConstraintScorer`, `ReportGenerator`
- Update report message strings (e.g., "Teacher split for group 5A" → "Teacher split for class 5A")

---

## Step 11: API Layer — Rename Endpoints

### 11.1 Rename endpoint files

| Old File | New File |
|----------|----------|
| `GradeEndpoints.cs` | `YearEndpoints.cs` |
| `GroupEndpoints.cs` | `ClassEndpoints.cs` |
| `TeachingDayEndpoints.cs` | `SchoolDayEndpoints.cs` |
| `GradeSubjectRequirementEndpoints.cs` | `YearCurriculumEndpoints.cs` |
| `GradeDayConfigEndpoints.cs` | `YearDayConfigEndpoints.cs` |

### 11.2 Rename API route paths

| Old Route | New Route |
|-----------|-----------|
| `/api/v1/grades` | `/api/v1/years` |
| `/api/v1/grades/bulk` | `/api/v1/years/bulk` |
| `/api/v1/grades/{gradeId}` | `/api/v1/years/{yearId}` |
| `/api/v1/grades/{gradeId}/groups` | `/api/v1/years/{yearId}/classes` |
| `/api/v1/grades/{gradeId}/groups/{id}` | `/api/v1/years/{yearId}/classes/{id}` |
| `/api/v1/grades/{gradeId}/subject-requirements` | `/api/v1/years/{yearId}/curriculum` |
| `/api/v1/grades/{gradeId}/subject-requirements/bulk` | `/api/v1/years/{yearId}/curriculum/bulk` |
| `/api/v1/grades/{gradeId}/subject-requirements/{id}` | `/api/v1/years/{yearId}/curriculum/{id}` |
| `/api/v1/grades/{gradeId}/day-config` | `/api/v1/years/{yearId}/day-config` |
| `/api/v1/grades/{gradeId}/day-config/{id}` | `/api/v1/years/{yearId}/day-config/{id}` |
| `/api/v1/grades/{gradeId}/combined-lessons` | `/api/v1/years/{yearId}/combined-lessons` |
| `/api/v1/grades/{gradeId}/combined-lessons/{id}` | `/api/v1/years/{yearId}/combined-lessons/{id}` |
| `/api/v1/teaching-days` | `/api/v1/school-days` |
| `/api/v1/teaching-days/{dayId}` | `/api/v1/school-days/{dayId}` |
| `/api/v1/teaching-days/{dayId}/time-slots` | `/api/v1/school-days/{dayId}/time-slots` |
| `/api/v1/teaching-days/{dayId}/time-slots/bulk` | `/api/v1/school-days/{dayId}/time-slots/bulk` |
| `/api/v1/teaching-days/{dayId}/time-slots/{id}` | `/api/v1/school-days/{dayId}/time-slots/{id}` |
| `/api/v1/timetables/{id}/by-group/{groupId}` | `/api/v1/timetables/{id}/by-class/{classId}` |
| `/api/v1/timetables/{id}/entries?groupId=` | `/api/v1/timetables/{id}/entries?classId=` |
| `/api/v1/timetables/{id}/entries?teachingDayId=` | `/api/v1/timetables/{id}/entries?schoolDayId=` |

### 11.3 Update endpoint method names

- `MapGradeEndpoints()` → `MapYearEndpoints()`
- `MapGroupEndpoints()` → `MapClassEndpoints()`
- `MapTeachingDayEndpoints()` → `MapSchoolDayEndpoints()`
- `MapGradeSubjectRequirementEndpoints()` → `MapYearCurriculumEndpoints()`
- `MapGradeDayConfigEndpoints()` → `MapYearDayConfigEndpoints()`

### 11.4 Update Program.cs

Update all `app.MapXxxEndpoints()` calls to use new names.

### 11.5 Update Swagger metadata

Update all `.WithSummary()`, `.WithDescription()`, `.WithTags()` to use new terminology:
- Tag "Grades" → "Years"
- Tag "Groups" → "Classes"  
- Tag "Teaching Days" → "School Days"
- Summary text: e.g., "List all grades" → "List all years"

---

## Step 12: API Layer — Update Timetable Endpoints

The timetable endpoints (`TimetableEndpoints.cs`) reference Grade/Group in:
- Query parameter names (`groupId` → `classId`, `teachingDayId` → `schoolDayId`)
- Route paths (`by-group` → `by-class`)
- Swagger summaries and descriptions
- Internal variable names

---

## Step 13: Seed Data

Update `SeedData.SeedDemoSchoolAsync` to use new entity names and any hard-coded strings.

---

## Step 14: EF Migration

After ALL code changes compile successfully:

```bash
dotnet ef migrations add RenameGradeToYearGroupToClass --project src/ClassForge.Infrastructure --startup-project src/ClassForge.API
```

Review the generated migration to ensure it correctly renames:
- Tables
- Columns (e.g., `GradeId` → `YearId`)
- Indexes
- Foreign key constraints

If EF generates drop+recreate instead of rename operations, manually edit the migration to use `migrationBuilder.RenameTable()`, `migrationBuilder.RenameColumn()`, and `migrationBuilder.RenameIndex()` to preserve data.

**IMPORTANT:** After generating the migration, inspect it carefully. EF Core sometimes generates `DropTable` + `CreateTable` instead of `RenameTable` for entity renames. You MUST change these to `RenameTable`/`RenameColumn`/`RenameIndex` operations to avoid data loss. Example:

```csharp
// BAD (data loss):
migrationBuilder.DropTable(name: "Grades");
migrationBuilder.CreateTable(name: "Years", ...);

// GOOD (preserves data):
migrationBuilder.RenameTable(name: "Grades", newName: "Years");
migrationBuilder.RenameColumn(name: "GradeId", table: "Groups", newName: "YearId");
```

---

## Step 15: Update Tests

### Unit Tests
- Rename all references in `TestDataBuilder` (e.g., building `Grade` → `Year`, `Group` → `Class`)
- Update test class/method names that reference old terminology
- Update string assertions on report messages

### Integration Tests
- Update API route paths in test HTTP calls
- Update DTO type references
- Update assertion property names
- Update `CustomWebApplicationFactory` if it references old entity names

---

## Step 16: Update Swagger JSON

After all changes are complete and the API runs:
1. Start the API: `dotnet run --project src/ClassForge.API`
2. Download the updated swagger spec from `/swagger/v1/swagger.json`
3. Replace the `swagger.json` file in the requirements folder

---

## Step 17: Update CLAUDE.md and README.md

Update both documentation files to reflect all new terminology. This includes:
- Entity names, route paths, endpoint descriptions
- Architecture descriptions
- API overview tables
- Any mentions of Grade/Group/TeachingDay/SubjectRequirement

---

## Verification Checklist

After completing all steps:

- [ ] `dotnet build ClassForge.sln` — zero errors
- [ ] `dotnet test` — all tests pass
- [ ] `dotnet run --project src/ClassForge.API` — API starts
- [ ] `/swagger` — all endpoints show new routes and DTO names
- [ ] `/health` — returns healthy
- [ ] No remaining references to old terms (run: `grep -r "Grade\|TeachingDay\|SubjectRequirement" --include="*.cs" -l` and verify each hit is a false positive or intentional)
- [ ] Search for `[Gg]rade` across all .cs files — only hits should be in migration history files (which should NOT be modified)
- [ ] Search for `[Gg]roup` across all .cs files — note that "group" may appear in LINQ `.GroupBy()` calls which are fine; check for entity/DTO references only
- [ ] EF migration generated and reviewed for rename (not drop+create) operations
- [ ] Seed data runs successfully in Development mode
- [ ] Swagger JSON re-exported
