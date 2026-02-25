# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# ClassForge Web — Developer Guide

## Overview

Next.js 16 multi-tenant SaaS frontend for Norwegian school timetabling.
API backend at `../ClassForge/` (ASP.NET Core, port 5208).

## Tech Stack

- **Next.js 16** + React 19 + TypeScript 5 + Tailwind CSS 4
- **API**: openapi-fetch (type-safe) + generated `src/lib/api/schema.ts`
- **State**: Zustand stores (auth, ui, wizard)
- **Data fetching**: TanStack React Query v5 with custom hooks in `src/lib/api/hooks/`
- **UI**: shadcn/ui + Radix UI + Lucide icons
- **i18n**: next-intl (locales: nb, nn, en — default: nb)
- **Forms**: react-hook-form + Zod

## Development

```bash
npm run dev      # Dev server → http://localhost:3000
npm run build    # Production build (must pass 0 TS errors)
npm run lint     # ESLint
```

No test framework is configured for the frontend.

**Required env:** `NEXT_PUBLIC_API_URL=http://localhost:5208/` (already in `.env.local`)

## Project Structure

```
src/
  app/
    [locale]/
      (auth)/       login, register (public)
      (app)/        dashboard, grades, subjects, rooms, teachers,
                    time-structure, timetables/[id]/report,
                    my-schedule, users, settings  (requires auth)
      layout.tsx    Locale root — Nunito font + NextIntlClientProvider + QueryProvider
      setup/        Onboarding wizard (public after register)
    layout.tsx      Root layout (children only)
    page.tsx        Redirects → /nb
    globals.css     Brand CSS vars + Tailwind
  components/
    layout/         Sidebar, Header, MobileNav
    auth/           LoginForm, RegisterForm
    providers/      QueryProvider, AuthInitializer, ForbiddenToastListener
    setup/          WizardShell + step-0..step-7 components
    time-structure/ PeriodTemplateBuilder, WeeklyCalendarGrid
    timetable/      TimetableGrid, TimetableCell, QualityGauge, CellEditPopover
    report/         ReportSummary, ReportItemList
    export/         excel-export.ts, print-layout.tsx
    ui/             shadcn/ui components
  lib/
    api/
      schema.ts     Auto-generated OpenAPI types — DO NOT EDIT
      client.ts     openapi-fetch client + auth interceptor
      hooks/        use-auth, use-grades, use-subjects, use-rooms,
                    use-teachers, use-teaching-days, use-timetables,
                    use-timetable-views, use-users, use-setup
    stores/         auth-store.ts, ui-store.ts, wizard-store.ts
    utils/          cn (shadcn), color.ts (20 subject colors)
  i18n/
    config.ts       locales array + localeNames
    routing.ts      defineRouting
    request.ts      Server-side message loading
  messages/
    nb/ nn/ en/     11 JSON namespaces each
  proxy.ts          next-intl locale routing + cf_has_session cookie guard
```

## Key Conventions

### "use client" rules
- Any component using hooks, state, event handlers, or `useTranslations` needs `"use client"`
- Page components can be async server components (await params) — use `getTranslations` from `"next-intl/server"`
- All `lib/api/hooks/` files are `"use client"`

### API call pattern
```ts
// In a hook file (always "use client"):
export function useGrades() {
  return useQuery({
    queryKey: ["grades"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/v1/grades");
      if (error) throw error;
      return (data ?? []) as GradeResponse[];   // ← nullish coalescing required
    },
  });
}
```
Schema types are often `string | null | undefined` — always use `?? ""` or `?? []`.

### Per-row component extraction
When a hook takes an entity ID as an **argument** (e.g. `useGroups(gradeId)`, `useCreateGroup(gradeId)`), it cannot be called inside a `.map()` loop — that violates Rules of Hooks. Extract a dedicated row/item component that receives the ID as a prop and calls the hooks internally:

```tsx
// ✗ Wrong — hooks inside a loop
grades.map((g) => { const groups = useGroups(g.id!); … })

// ✓ Correct — one component instance per row
function GradeRow({ grade }: { grade: GradeResponse }) {
  const { data: groups } = useGroups(grade.id!);
  const createGroup = useCreateGroup(grade.id!);
  // …
}
grades.map((g) => <GradeRow key={g.id} grade={g} />)
```

This pattern is used in `grades/page.tsx` (`GradeRow`) and `step-1-grades.tsx` (`GradeItem`). Contrast with `useUpdateGroup`/`useDeleteGroup`, which take the IDs as mutation variables (not hook args) and can be called once at any level.

### Parallel fetching with useQueries
When a page needs data for a dynamic list of resources (e.g. time slots for each active teaching day), use `useQueries` — calling a single hook inside a loop violates Rules of Hooks:

```ts
import { useQueries } from "@tanstack/react-query";

const results = useQueries({
  queries: items.map((item) => ({
    queryKey: ["resource", item.id, "sub"],
    queryFn: async () => { /* ... */ },
    enabled: !!item.id,
  })),
});
const dataByItem = new Map(items.map((item, i) => [item.id!, results[i]?.data ?? []]));
```

### Auth flow
1. Login → JWT parsed → Zustand auth store updated → `cf_has_session=1` cookie set
2. `proxy.ts` checks the cookie for protected routes, redirects to `/[locale]/login` if missing
3. `apiClient` auto-attaches `Authorization: Bearer` header
4. On 401 → refresh → retry → on failure clear token + redirect to login
5. On 403 → `classforge:forbidden` event → sonner toast

### i18n
```ts
// Server component:
const t = await getTranslations("auth");
// Client component:
const t = useTranslations("auth");
```
Translation files: `messages/{nb,nn,en}/{namespace}.json`

### shadcn/ui
Install new components: `npx shadcn@latest add <component>`
All components land in `src/components/ui/`.

### List item rows
Two row styles are used consistently across CRUD list pages:

**Basic row** (grades, rooms):
```tsx
<div className="flex items-center justify-between px-4 py-3 rounded-xl border bg-card shadow-sm">
```

**Color-stripe row** (subjects):
```tsx
<div className="flex items-stretch rounded-xl border bg-card shadow-sm overflow-hidden">
  <div className="w-3 shrink-0" style={{ backgroundColor: color }} />
  <div className="flex flex-1 items-center justify-between px-4 py-3">
```

### Inline edit pattern
Grades, subjects, rooms, and groups use the same inline editing approach: a local `editingId` state drives a conditional render — display row vs edit row. Edit rows use `h-8 text-sm` inputs, a green `<Check>` ghost button (`text-green-600 hover:text-green-700 hover:bg-green-50`), and a muted `<X>` ghost button. The pencil trigger uses `variant="outline"` with muted text, matching the destructive delete button's border style.

## Important Gotchas

| Issue | Detail |
|-------|--------|
| `proxy.ts` not `middleware.ts` | next-intl requires the middleware file to be named `proxy.ts` in this project — `middleware.ts` is not used |
| Wizard file names ≠ positions | `step-2-subjects.tsx` renders at position 3; `step-3-rooms.tsx` at position 2. Actual order: 0 Template, 1 Grades, 2 Rooms, 3 Subjects, 4 Time, 5 Teachers, 6 Curriculum, 7 Review. Use position numbers when calling `markStepCompleted`/`setCurrentStep`. |
| `TeacherResponse.name` | API has `name` (not `firstName`/`lastName`) |
| `GradeResponse` has no `groups` | Fetch groups separately via `useGroups(gradeId)` and `useCreateGroup(gradeId)` (hook-arg pattern); `useUpdateGroup`/`useDeleteGroup` take IDs as mutation variables instead |
| `schema.ts` is auto-generated | Re-generate with: `npx openapi-typescript requirements/swagger.json -o src/lib/api/schema.ts` |
| Access token in memory | Refresh token in localStorage; access token never persisted |
| Timetable polling | `refetchInterval: 2000` when `status === "Generating"` |
| Time display formatting | Use `Intl.DateTimeFormat(undefined, { hour: "numeric", minute: "2-digit" })` to display times — never interpolate raw `HH:mm` strings, as this ignores the user's 12/24h system preference |
| Template seeding | `step-0-template.tsx` bulk-creates grades/subjects on non-custom template selection and marks wizard steps completed. Bulk hooks (`useBulkCreateGrades`, `useBulkCreateSubjects`) follow the same pattern as `useBulkCreateTimeSlots` in `use-teaching-days.ts`. |

## Brand Colors

| Token | Hex | Usage |
|-------|-----|-------|
| Firefly | `#0F2830` | Sidebar background, dark text |
| Emerald | `#014751` | Secondary dark |
| Green | `#00D37F` | Primary / CTA |
| Mint | `#AFF8C8` | Secondary / accent |
| Zircon | `#F8FBFF` | Page background |

20 subject palette colors available as `--subject-01` … `--subject-20` CSS vars and `SUBJECT_COLORS[]` in `src/lib/utils/color.ts`.
