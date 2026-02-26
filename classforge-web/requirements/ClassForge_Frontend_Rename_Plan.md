# ClassForge Frontend — Terminology Refactoring Plan

## Overview

Rename four domain terms throughout the frontend codebase to match the updated backend API:

| Old Term | New Term | Old API Route | New API Route |
|----------|----------|---------------|---------------|
| Grade | Year | `/api/v1/grades` | `/api/v1/years` |
| Group | Class | `/api/v1/grades/{id}/groups` | `/api/v1/years/{id}/classes` |
| Teaching Day | School Day | `/api/v1/teaching-days` | `/api/v1/school-days` |
| Subject Requirement | Curriculum | `/api/v1/grades/{id}/subject-requirements` | `/api/v1/years/{id}/curriculum` |

**PREREQUISITE:** The backend refactoring must be complete and a new `swagger.json` must be exported before starting this plan. The frontend relies on auto-generated types from the swagger spec.

## Execution Order

Complete each step fully before moving to the next. Run `npm run build` after each major step to catch TypeScript errors early.

---

## Step 1: Regenerate API Types

1. Copy the updated `swagger.json` from the backend into `requirements/swagger.json`
2. Regenerate types:
   ```bash
   npx openapi-typescript requirements/swagger.json -o src/lib/api/schema.ts
   ```
3. Run `npm run build` — this will produce many TypeScript errors. These errors are your roadmap for the remaining steps.

---

## Step 2: Rename API Hook Files & Exports

### 2.1 Rename hook files

| Old File | New File |
|----------|----------|
| `src/lib/api/hooks/use-grades.ts` | `src/lib/api/hooks/use-years.ts` |
| `src/lib/api/hooks/use-teaching-days.ts` | `src/lib/api/hooks/use-school-days.ts` |

Note: If there is a separate `use-groups.ts`, rename it to `use-classes.ts`. If groups are part of `use-grades.ts`, they'll move into `use-years.ts`.

### 2.2 Rename hook functions inside files

| Old Hook | New Hook |
|----------|----------|
| `useGrades()` | `useYears()` |
| `useGrade(id)` | `useYear(id)` |
| `useCreateGrade()` | `useCreateYear()` |
| `useUpdateGrade()` | `useUpdateYear()` |
| `useDeleteGrade()` | `useDeleteYear()` |
| `useBulkCreateGrades()` | `useBulkCreateYears()` |
| `useGroups(gradeId)` | `useClasses(yearId)` |
| `useCreateGroup()` | `useCreateClass()` |
| `useUpdateGroup()` | `useUpdateClass()` |
| `useDeleteGroup()` | `useDeleteClass()` |
| `useTeachingDays()` | `useSchoolDays()` |
| `useCreateTeachingDay()` | `useCreateSchoolDay()` |
| `useUpdateTeachingDay()` | `useUpdateSchoolDay()` |
| `useDeleteTeachingDay()` | `useDeleteSchoolDay()` |
| `useTimeSlots(teachingDayId)` | `useTimeSlots(schoolDayId)` |
| `useBulkCreateTimeSlots()` | (keep name, update parameter names) |
| `useSubjectRequirements(gradeId)` | `useCurriculum(yearId)` |
| `useCreateSubjectRequirement()` | `useCreateCurriculumEntry()` |
| `useUpdateSubjectRequirement()` | `useUpdateCurriculumEntry()` |
| `useDeleteSubjectRequirement()` | `useDeleteCurriculumEntry()` |
| `useBulkCreateSubjectRequirements()` | `useBulkCreateCurriculum()` |
| `useGradeDayConfigs(gradeId)` | `useYearDayConfigs(yearId)` |
| `useCreateGradeDayConfig()` | `useCreateYearDayConfig()` |
| `useUpdateGradeDayConfig()` | `useUpdateYearDayConfig()` |
| `useDeleteGradeDayConfig()` | `useDeleteYearDayConfig()` |
| `useCombinedLessons(gradeId)` | `useCombinedLessons(yearId)` (keep name, update param) |
| `useTimetableByGroup(...)` | `useTimetableByClass(...)` |

### 2.3 Update API paths inside hooks

Update all `apiClient.GET(...)` and `apiClient.POST(...)` etc. calls:

| Old Path | New Path |
|----------|----------|
| `"/api/v1/grades"` | `"/api/v1/years"` |
| `"/api/v1/grades/bulk"` | `"/api/v1/years/bulk"` |
| `"/api/v1/grades/{gradeId}"` | `"/api/v1/years/{yearId}"` |
| `"/api/v1/grades/{gradeId}/groups"` | `"/api/v1/years/{yearId}/classes"` |
| `"/api/v1/grades/{gradeId}/groups/{id}"` | `"/api/v1/years/{yearId}/classes/{id}"` |
| `"/api/v1/grades/{gradeId}/subject-requirements"` | `"/api/v1/years/{yearId}/curriculum"` |
| `"/api/v1/grades/{gradeId}/subject-requirements/bulk"` | `"/api/v1/years/{yearId}/curriculum/bulk"` |
| `"/api/v1/grades/{gradeId}/subject-requirements/{id}"` | `"/api/v1/years/{yearId}/curriculum/{id}"` |
| `"/api/v1/grades/{gradeId}/day-config"` | `"/api/v1/years/{yearId}/day-config"` |
| `"/api/v1/grades/{gradeId}/day-config/{id}"` | `"/api/v1/years/{yearId}/day-config/{id}"` |
| `"/api/v1/grades/{gradeId}/combined-lessons"` | `"/api/v1/years/{yearId}/combined-lessons"` |
| `"/api/v1/grades/{gradeId}/combined-lessons/{id}"` | `"/api/v1/years/{yearId}/combined-lessons/{id}"` |
| `"/api/v1/teaching-days"` | `"/api/v1/school-days"` |
| `"/api/v1/teaching-days/{dayId}"` | `"/api/v1/school-days/{dayId}"` |
| `"/api/v1/teaching-days/{dayId}/time-slots"` | `"/api/v1/school-days/{dayId}/time-slots"` |
| `"/api/v1/teaching-days/{dayId}/time-slots/bulk"` | `"/api/v1/school-days/{dayId}/time-slots/bulk"` |
| `"/api/v1/teaching-days/{dayId}/time-slots/{id}"` | `"/api/v1/school-days/{dayId}/time-slots/{id}"` |
| `"/api/v1/timetables/{id}/by-group/{groupId}"` | `"/api/v1/timetables/{id}/by-class/{classId}"` |
| `"/api/v1/timetables/{id}/entries?groupId="` | `"/api/v1/timetables/{id}/entries?classId="` |
| `"/api/v1/timetables/{id}/entries?teachingDayId="` | `"/api/v1/timetables/{id}/entries?schoolDayId="` |

### 2.4 Update TanStack Query keys

| Old Key | New Key |
|---------|---------|
| `["grades"]` | `["years"]` |
| `["grades", gradeId, "groups"]` | `["years", yearId, "classes"]` |
| `["grades", gradeId, "subject-requirements"]` | `["years", yearId, "curriculum"]` |
| `["grades", gradeId, "day-config"]` | `["years", yearId, "day-config"]` |
| `["grades", gradeId, "combined-lessons"]` | `["years", yearId, "combined-lessons"]` |
| `["teaching-days"]` | `["school-days"]` |
| `["teaching-days", dayId, "time-slots"]` | `["school-days", dayId, "time-slots"]` |
| `["timetables", id, "by-group", groupId]` | `["timetables", id, "by-class", classId]` |

Also update all query invalidation calls (`queryClient.invalidateQueries`) that reference old keys.

### 2.5 Update TypeScript type aliases

If there are type aliases or imports from `schema.ts` like:
```ts
type GradeResponse = components["schemas"]["GradeResponse"];
```
Rename to:
```ts
type YearResponse = components["schemas"]["YearResponse"];
```

Do this for ALL DTOs listed in Step 2.2 of the backend plan.

---

## Step 3: Rename Page Routes & Files

### 3.1 Rename route directories

| Old Path | New Path |
|----------|----------|
| `src/app/[locale]/(app)/grades/` | `src/app/[locale]/(app)/years/` |

Note: If the frontend has a Groups page (likely nested within Grades), it moves with the Years folder. The routes for subjects, rooms, teachers, time-structure, timetables do NOT change — only grades → years.

### 3.2 Update navigation/sidebar

In the sidebar component (`src/components/layout/sidebar.tsx`):
- Update route path: `/grades` → `/years`
- Update label translation key: `common.grades` → `common.years` (or whatever key is used)
- Update icon if the label reference changes

### 3.3 Update any `router.push()` or `<Link>` references

Search the entire codebase for:
- `/grades` → `/years`
- Links referencing `grades` route segments

---

## Step 4: Rename Components

### 4.1 Rename component files and directories

| Old | New |
|-----|-----|
| `src/components/grades/` (if exists) | `src/components/years/` |
| `grade-card.tsx` | `year-card.tsx` |
| `group-list.tsx` | `class-list.tsx` |
| Any component with "grade" or "group" in the filename | Rename accordingly |

### 4.2 Rename component exports

Inside each renamed file:
- `GradeCard` → `YearCard`
- `GroupList` → `ClassList`
- `GradeSelector` → `YearSelector`
- `GroupSelector` → `ClassSelector`
- etc.

### 4.3 Update all imports across the codebase

Every file that imports from the old paths/names needs updating.

---

## Step 5: Rename Setup Wizard Steps

The wizard has steps that reference grades and groups. Based on the CLAUDE.md, the step files are:
- `step-0-template.tsx` — references grades/subjects in template seeding
- `step-1-grades.tsx` → rename to `step-1-years.tsx`
- Files for groups (might be within the grades step or separate)
- `step-6-curriculum.tsx` — already named curriculum, but may reference "subject requirements" internally

### 5.1 Rename step files

| Old | New |
|-----|-----|
| `step-1-grades.tsx` (or wherever grades step is) | `step-1-years.tsx` |

### 5.2 Update internal references

Inside each wizard step component:
- `grades` → `years` (variable names, state, API calls)
- `groups` → `classes`
- `gradeId` → `yearId`
- `groupId` → `classId`
- `teachingDays` → `schoolDays`
- `subjectRequirements` → `curriculum`

### 5.3 Update WizardShell step labels

If `wizard-shell.tsx` defines step labels, update:
- "Grades & Groups" → "Years & Classes" (or use translation keys)

### 5.4 Update wizard store

In `src/lib/stores/wizard-store.ts`:
- Rename any state properties referencing old terms
- Note from CLAUDE.md: "Wizard file names ≠ positions" — be careful to preserve the position mapping

---

## Step 6: Update Timetable Components

### 6.1 Timetable grid and cell components

In `src/components/timetable/`:
- `TimetableGrid`: update props and variable names (`groupId` → `classId`, etc.)
- `TimetableCell`: update displayed data (`groupNames` → `classNames` from the API response)
- `CellEditPopover`: update any references
- `GroupSelector` → `ClassSelector` (or rename within view-switcher)

### 6.2 Timetable view hooks

- `useTimetableByGroup` → `useTimetableByClass`
- Update the view type enum/state: `"group"` → `"class"` in the UI store's `timetableView`

### 6.3 Update report components

In `src/components/report/`:
- Report items may reference "group" in category names or messages — these come from the API so they'll auto-update, but check any frontend-side label mapping

---

## Step 7: Update Time Structure Components

In `src/components/time-structure/` (or wherever these live):
- Rename references from `teachingDay` → `schoolDay` in all component props, variables, and function names
- `PeriodTemplateBuilder`: if it references teaching days by name
- `WeeklyCalendarGrid`: update prop names

---

## Step 8: Update Zustand Stores

### 8.1 UI Store (`src/lib/stores/ui-store.ts`)

```
timetableView: "group" | "teacher" | "master"
→
timetableView: "class" | "teacher" | "master"
```

Update `selectedGroupId` → `selectedClassId` (and any related actions).

### 8.2 Wizard Store

Update any properties referencing old terms.

---

## Step 9: Update Translation Files

### 9.1 Rename translation namespaces/files

| Old | New |
|-----|-----|
| `messages/{locale}/grades.json` | `messages/{locale}/years.json` |

If there are separate files for `groups.json`, `teachingDays.json`, `subjectRequirements.json`, rename those too.

### 9.2 Update translation keys within files

This is the most labor-intensive part. In every locale (`nb`, `nn`, `en`):

**In the years (was grades) namespace:**
- All keys referencing "grade"/"grades" should now reference "year"/"years"
- All keys referencing "group"/"groups" should now reference "class"/"classes"

**In the common namespace:**
- Navigation labels: `"grades": "Klassetrinn"` → `"years": "Trinn"` (nb), `"Trinn"` (nn), `"Years"` (en)
- Navigation labels: `"groups": "Klasser"` → `"classes": "Klasser"` (nb/nn), `"Classes"` (en)

**In the timeStructure namespace:**
- `"teachingDays"` → `"schoolDays"`, with Norwegian: `"Skoledager"` (nb/nn), `"School Days"` (en)

**In the timetable namespace:**
- "Group view" → "Class view" / "Klassevisning"
- `"byGroup"` → `"byClass"`

**In the setup namespace:**
- Step labels and descriptions referencing grades/groups

**Specific Norwegian translations:**

| English (en) | Bokmål (nb) | Nynorsk (nn) |
|--------------|-------------|--------------|
| Years | Trinn | Trinn |
| Year | Trinn | Trinn |
| Classes | Klasser | Klassar |
| Class | Klasse | Klasse |
| School Days | Skoledager | Skuledagar |
| School Day | Skoledag | Skuledag |
| Curriculum | Fag- og timefordeling | Fag- og timefordeling |

### 9.3 Update all `useTranslations()` and `getTranslations()` calls

If the namespace changed (e.g., `useTranslations("grades")` → `useTranslations("years")`), update every component that imports from the old namespace.

Also update all translation key references:
- `t("grades.title")` → `t("years.title")`
- `t("grades.addGroup")` → `t("years.addClass")`
- etc.

---

## Step 10: Update Export Components

In `src/components/export/`:
- `excel-export.ts`: update column headers, sheet names referencing "Group" → "Class"
- `print-layout.tsx`: update any labels
- PDF export: update headers/labels

---

## Step 11: Update the `proxy.ts` (Middleware)

Check if `proxy.ts` has any route patterns referencing `/grades` that need to change to `/years`.

---

## Step 12: Update CLAUDE.md and README.md

Update both documentation files to reflect all new terminology:
- Route paths
- Component names and file paths
- Hook names
- Store properties
- Translation namespace names
- The "Important Gotchas" table
- API overview tables

---

## Verification Checklist

After completing all steps:

- [ ] `npm run build` — zero TypeScript errors
- [ ] `npm run lint` — no new lint errors
- [ ] All pages load correctly at the new routes (`/years` instead of `/grades`)
- [ ] Sidebar navigation shows updated labels in all three languages (nb, nn, en)
- [ ] Setup wizard correctly uses new API endpoints (bulk create years, classes, etc.)
- [ ] Timetable views correctly switch between Class View / Teacher View / Master View
- [ ] Cell editing popover works with new field names
- [ ] Export (Excel, PDF, Print) uses updated terminology
- [ ] No remaining references to old terms (run searches):
  - `grep -r "useGrade\|useGroup\|useTeachingDay\|useSubjectRequirement" src/ --include="*.ts" --include="*.tsx" -l`
  - `grep -r '"/api/v1/grades\|/api/v1/teaching-days\|by-group\|subject-requirements"' src/ --include="*.ts" --include="*.tsx" -l`
  - `grep -r '"grade"\|"group"\|"teachingDay"' src/messages/ --include="*.json" -l`
- [ ] Note: `"group"` may appear legitimately in non-domain contexts (e.g., CSS `group` class, `groupBy` utility functions). Only domain-entity references should be renamed.
- [ ] All three locales (nb, nn, en) render correctly with updated terms
- [ ] API communication works end-to-end (create year → add classes → generate timetable → view)
