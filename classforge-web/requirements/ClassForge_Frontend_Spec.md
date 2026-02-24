# ClassForge — Frontend Requirements Specification

**Next.js 16 (App Router) | TypeScript | Tailwind CSS 4 | shadcn/ui**

Version 1.0 — February 2026
_Prepared for development with Claude Code_

---

## 1. Project Overview

The ClassForge frontend is a web application that provides school administrators and schedule managers with an intuitive interface to configure their school structure, manage teachers, and generate optimized weekly timetables. Teachers can view their schedules through a responsive mobile-friendly view.

The frontend consumes the ClassForge REST API (see `swagger.json` for full API reference).

### 1.1 Technology Stack

- **Framework:** Next.js 16 Latest (App Router)
- **Language:** TypeScript (strict mode)
- **Styling:** Tailwind CSS 4
- **Component Library:** shadcn/ui (built on Radix UI)
- **Server State:** TanStack Query v5 (React Query)
- **Client State:** Zustand
- **Forms:** React Hook Form + Zod validation
- **Tables:** TanStack Table v8
- **i18n:** next-intl (supports locale-based routing)
- **API Types:** openapi-typescript (auto-generated from swagger.json)
- **API Client:** openapi-fetch (type-safe fetch client from openapi-typescript)
- **Icons:** Lucide React
- **Date/Time:** date-fns
- **Charts:** Recharts (for quality score visualization)
- **PDF Export:** @react-pdf/renderer
- **Excel Export:** SheetJS (xlsx)
- **Print:** react-to-print
- **Font:** Nunito (primary — approachable, rounded, educational feel)
- **Animations:** Framer Motion (subtle, purposeful transitions only)
- **Package Manager:** pnpm

### 1.2 Browser Support

- Chrome/Edge (latest 2 versions)
- Firefox (latest 2 versions)
- Safari (latest 2 versions)
- Mobile Safari and Chrome (for teacher timetable view only)

---

## 2. Design System

### 2.1 Brand Colors

```
Primary Palette:
  Firefly:       #0F2830  — sidebar bg, dark surfaces, primary text on light bg
  Emerald:       #014751  — sidebar hover/active, secondary dark surfaces
  Green:         #00D37F  — primary action (buttons, links, active indicators)
  Mint:          #AFF8C8  — success states, light highlights, hover tints
  Zircon:        #F8FBFF  — main content background
  Banana Yellow: #FFEEB4  — warnings, attention badges
  Lilac:         #D2C4FB  — info badges, secondary accents, tags

Derived/Utility:
  Green Dark:    #00B36B  — button hover state (darken Green by 10%)
  Green Light:   #E6FFF3  — green tint for backgrounds/selections
  Red:           #EF4444  — errors, destructive actions, delete buttons
  Red Light:     #FEF2F2  — error backgrounds
  Yellow Dark:   #D4A017  — warning text on light backgrounds
  Gray 50:       #F9FAFB  — subtle backgrounds, table alternating rows
  Gray 100:      #F3F4F6  — borders, dividers
  Gray 200:      #E5E7EB  — input borders
  Gray 300:      #D1D5DB  — disabled states
  Gray 500:      #6B7280  — secondary text, placeholders
  Gray 700:      #374151  — body text
  Gray 900:      #111827  — headings
```

### 2.2 Subject Color Palette

A set of 20 distinct, soft colors designed to work as timetable cell backgrounds with dark text. When a new subject is created, assign the next unused color sequentially. Users can override via a color picker.

```
subject-01: #DBEAFE  (soft blue)
subject-02: #DCF5E7  (soft green)
subject-03: #FEF3C7  (soft amber)
subject-04: #EDE9FE  (soft violet)
subject-05: #FCE7F3  (soft pink)
subject-06: #CFFAFE  (soft cyan)
subject-07: #FEE2E2  (soft red)
subject-08: #F0FDF4  (soft emerald)
subject-09: #FFF7ED  (soft orange)
subject-10: #F5F3FF  (soft purple)
subject-11: #ECFDF5  (soft teal)
subject-12: #FDF4FF  (soft fuchsia)
subject-13: #F0F9FF  (soft sky)
subject-14: #FFFBEB  (soft yellow)
subject-15: #F1F5F9  (soft slate)
subject-16: #FFF1F2  (soft rose)
subject-17: #E0F2FE  (soft light blue)
subject-18: #D1FAE5  (soft light green)
subject-19: #FDE68A  (soft gold)
subject-20: #E9D5FF  (soft lavender)
```

### 2.3 Typography

```
Font Family: "Nunito", sans-serif (load via next/font/google)
  — Rounded, friendly, highly legible at all sizes
  — Weights: 400 (body), 600 (semibold labels), 700 (bold headings), 800 (extrabold display)

Scale:
  xs:   12px / 1.5   — captions, badges
  sm:   14px / 1.5   — secondary text, table cells
  base: 16px / 1.5   — body text, form inputs
  lg:   18px / 1.5   — card titles, section labels
  xl:   20px / 1.4   — page subtitles
  2xl:  24px / 1.3   — page titles
  3xl:  30px / 1.2   — feature headings (setup wizard)
  4xl:  36px / 1.1   — marketing/login display
```

### 2.4 Component Styling Guidelines

- **Buttons (primary):** bg Green (#00D37F), white text, rounded-lg, hover: Green Dark, subtle scale transform on press
- **Buttons (secondary):** border Gray 200, hover bg Gray 50
- **Buttons (destructive):** bg Red, white text
- **Cards:** bg white, border Gray 100, rounded-xl, shadow-sm
- **Inputs:** border Gray 200, rounded-lg, focus ring Green with 2px offset
- **Sidebar:** bg Firefly, text white, active item bg Emerald with Green left border accent
- **Tables:** Header bg Gray 50, alternating row bg Gray 50/white, border Gray 100
- **Modals/Dialogs:** Centered, max-w-lg default, backdrop blur-sm
- **Toasts:** Bottom-right, slide-in animation, auto-dismiss 5s
- **Badges:** Rounded-full, small text, color-coded (Green=success, Banana Yellow=warning, Red=error, Lilac=info)

### 2.5 Spacing & Layout

- Content max-width: 1280px (centered within main area)
- Sidebar width: 260px (collapsible to 64px icon-only on desktop)
- Page padding: 24px (desktop), 16px (mobile)
- Card padding: 24px
- Section spacing: 32px between major sections
- Form field spacing: 16px between fields
- Grid gap: 16px default

### 2.6 Motion

- Page transitions: fade-in 150ms
- Sidebar collapse: width transition 200ms ease
- Modals: fade + scale-in 200ms
- Toasts: slide-in from right 300ms
- Timetable cell hover: subtle bg opacity change 100ms
- Loading skeletons: shimmer animation for data loading states
- Keep all motion subtle and purposeful — this is a productivity tool, not a marketing site

---

## 3. Internationalization (i18n)

### 3.1 Architecture

- Use `next-intl` with locale-based routing: `/nb/dashboard`, `/nn/dashboard`, `/en/dashboard`
- Default locale: `nb` (Norwegian Bokmål)
- Supported locales: `nb` (Bokmål), `nn` (Nynorsk), `en` (English)
- Translation files: JSON per namespace per locale, e.g. `messages/nb/common.json`, `messages/nb/teachers.json`

### 3.2 Locale Resolution

1. User's personal language preference (stored in user profile / localStorage)
2. Tenant default language (from school settings, fetched via API)
3. Browser `Accept-Language` header
4. Fallback: `nb`

### 3.3 Namespaces

Organize translations by feature area:

```
common        — shared UI: buttons, labels, navigation, errors, confirmations
auth          — login, register, password reset
setup         — setup wizard steps
grades        — grades & groups management
subjects      — subjects management
rooms         — rooms management
teachers      — teacher management, qualifications, availability
timeStructure — teaching days, time slots, breaks
timetable     — generation, views, editing, reports
users         — user management
settings      — school settings
```

### 3.4 Content Rules

- All user-facing text must come from translation files — never hardcode strings
- Date formatting: use `date-fns` locale-aware formatting (Norwegian date format: `dd.MM.yyyy`)
- Number formatting: Norwegian uses comma as decimal separator, period as thousands separator
- Day names: Localized (Mandag, Tirsdag... not Monday, Tuesday...)
- Data entered by users (subject names, teacher names, etc.) is stored as-is — no translation

---

## 4. Authentication & Routing

### 4.1 Auth Flow

- Store JWT `accessToken` and `refreshToken` in memory (Zustand store) + `httpOnly` cookie for refresh token
- On app load: attempt token refresh via `/api/v1/auth/refresh`
- On 401 response: redirect to login
- Auto-refresh: refresh token 60 seconds before accessToken expiry
- OAuth: redirect to backend OAuth URL, handle callback with token exchange

### 4.2 Route Structure

```
/[locale]/                          → Redirect to /dashboard or /login
/[locale]/login                     → Login page (public)
/[locale]/register                  → Registration page (public)
/[locale]/setup                     → Setup wizard (post-registration, authenticated)
/[locale]/dashboard                 → Dashboard/home (authenticated)
/[locale]/grades                    → Grades & groups management
/[locale]/subjects                  → Subjects management
/[locale]/rooms                     → Rooms management
/[locale]/teachers                  → Teacher list
/[locale]/teachers/[id]             → Teacher detail (qualifications, availability, blocked slots)
/[locale]/time-structure            → Teaching days & time slots
/[locale]/timetables                → Timetable list
/[locale]/timetables/[id]           → Timetable viewer (group/teacher/master views)
/[locale]/timetables/[id]/report    → Quality report
/[locale]/users                     → User management (OrgAdmin only)
/[locale]/settings                  → School settings
/[locale]/my-schedule               → Teacher's own timetable (Viewer role, mobile-friendly)
```

### 4.3 Role-Based Access

| Route                                                  | OrgAdmin | ScheduleManager   | Viewer                   |
| ------------------------------------------------------ | -------- | ----------------- | ------------------------ |
| /dashboard                                             | Yes      | Yes               | Redirect to /my-schedule |
| /grades, /subjects, /rooms, /teachers, /time-structure | Yes      | Yes (read + edit) | No                       |
| /timetables (create/generate)                          | Yes      | Yes               | No                       |
| /timetables/[id] (view)                                | Yes      | Yes               | Published only           |
| /users                                                 | Yes      | No                | No                       |
| /settings                                              | Yes      | No                | No                       |
| /my-schedule                                           | Yes      | Yes               | Yes                      |

Implement route guards as Next.js middleware + client-side checks.

---

## 5. Page Specifications

### 5.1 Login Page

- Clean, centered card layout
- School name / ClassForge branding at top
- Email + password form
- "Sign in with Google" / "Sign in with Microsoft" OAuth buttons
- Link to registration
- Language selector in corner (small dropdown: Bokmål / Nynorsk / English)
- Background: subtle gradient using Firefly → Emerald

### 5.2 Registration Page

- School name, admin name, email, password, confirm password
- After successful registration, redirect to setup wizard
- Same visual style as login

### 5.3 Setup Wizard

A guided, multi-step onboarding flow. The user can skip any step and return later. Progress is saved after each step.

**Step 0 — School Profile & Template Selection**

- School name (pre-filled from registration)
- School type selector with pre-populated templates:
  - "Barneskole (1.–7. klasse)" — primary school
  - "Ungdomsskole (8.–10. klasse)" — lower secondary
  - "Barneskole + Ungdomsskole (1.–10. klasse)" — combined
  - "Videregående (VG1–VG3)" — upper secondary
  - "Custom / Egendefinert" — blank slate
- Selecting a template pre-fills subsequent steps (grades, common subjects, standard Mon-Fri schedule with typical Norwegian period structure)
- The user can always modify pre-filled data

**Step 1 — Grades & Groups**

- List of grades (pre-filled from template or empty)
- For each grade: name, number of groups (with auto-generated names A, B, C...)
- Add/remove grades
- Drag-to-reorder

**Step 2 — Subjects**

- List of subjects (pre-filled from template)
- For each: name, color (auto-assigned from subject palette, editable), allow double periods toggle, requires special room toggle
- Add/remove subjects

**Step 3 — Rooms**

- Only special rooms (skip if none needed)
- Name + capacity for each
- Link subjects to rooms

**Step 4 — Time Structure**

- Teaching days checkboxes (Mon-Fri pre-checked)
- Period builder: visual timeline for one day, add periods with start/end times, mark breaks
- Copy structure to all days (with option to customize per day)
- Grade day config: for each grade, set how many periods per day (table/matrix: grades × days)

**Step 5 — Curriculum Requirements**

- Matrix view: grades (rows) × subjects (columns)
- Each cell: periods per week (number input)
- Toggle for "prefer double periods" per cell
- Pre-filled from template if applicable
- Combined lesson config: per grade per subject, toggle combining, select which groups

**Step 6 — Teachers (optional in wizard)**

- Basic teacher list: name, email
- Quick subject assignment (checkboxes)
- Note: detailed qualification setup (grade ranges, availability, blocked slots) deferred to main teacher management
- "Skip for now" prominent option

**Step 7 — Review & Finish**

- Summary of everything configured
- Highlight any obvious gaps (e.g. "3 subjects have no teachers assigned")
- "Go to Dashboard" button
- "Generate First Timetable" shortcut (runs preflight + generate)

**Wizard UX:**

- Step indicator (horizontal stepper) at top showing all steps with completion status
- "Next" / "Back" / "Skip this step" buttons
- Auto-save on each step completion
- User can exit wizard at any time and resume from dashboard
- After wizard completion, a "Setup Wizard" link remains accessible from settings to re-run or review

### 5.4 Dashboard

- **Welcome banner** with school name and quick stats: total grades, groups, teachers, subjects
- **Quick Actions** card: "Generate Timetable", "Manage Teachers", "View Latest Timetable"
- **Recent Timetables** list showing name, status badge, quality score, generation date
- **Setup Completion** progress bar (if wizard not fully completed): "You've completed 4/7 setup steps"
- **Preflight Status** widget: shows last preflight result, or prompt to run one

### 5.5 Grades & Groups Page

- List of grades in a card layout, sorted by SortOrder
- Each grade card shows: name, group count, expand to see/manage groups
- Inline add/edit for grade names
- Within each grade: list groups, add/remove/rename
- Drag-to-reorder grades and groups within a grade
- Subject requirements matrix accessible per grade (or link to curriculum page)

### 5.6 Subjects Page

- Table/card list of all subjects
- Columns: color swatch, name, double periods (yes/no), special room (name or "—"), max periods/day
- Inline color picker (click swatch to change)
- Add/edit via modal dialog
- Search/filter by name

### 5.7 Rooms Page

- Simple table: name, capacity, linked subjects
- Add/edit/delete via modal
- Only shown if any rooms exist (otherwise, prompt to create first room or skip)

### 5.8 Teachers Page

- **List view:** Table with columns: name, email, subjects (badge list), status (configured/incomplete)
- A teacher is "incomplete" if they have no qualifications or no day config
- Click row → navigate to teacher detail page
- Bulk add: import from CSV (name, email columns)
- Search/filter by name, subject

**Teacher Detail Page (/teachers/[id]):**
Three tabs or sections:

_Qualifications tab:_

- List of subject qualifications showing: subject name, grade range (e.g. "3rd – 7th")
- Add qualification: select subject, select min grade, select max grade
- Edit/delete qualifications

_Availability tab:_

- Weekly grid (days × periods) showing available/unavailable status
- Set max periods per day via number input per day (0 = day off, shown as grayed out column)
- Visual indicator: green = available, gray = unavailable/day off

_Blocked Slots tab:_

- Same weekly grid view as availability but for specific slot blocks
- Click a slot to block/unblock it
- Blocked slots shown in red/orange with reason tooltip
- Add reason via small popover on block

### 5.9 Time Structure Page

- **Teaching days:** Toggle switches for each day of the week, with active/inactive state
- **Period builder:** Visual timeline for each active day
  - Show periods as blocks on a vertical timeline
  - Each block: slot number, start time, end time, type (lesson/break)
  - Add period (appends with auto-calculated start time based on previous end time)
  - Edit start/end times inline
  - Mark as break (visual change: hatched/gray)
  - Delete periods
  - "Copy this day's structure to all days" button
- **Grade Day Config:** Matrix (grades × days) with max period counts
  - Quick fill: "Set all to X" per grade or per day

### 5.10 Timetable List Page

- Table of all timetables: name, status (Draft/Published/Archived/Generating/Failed), quality score, created by, date
- Status shown as colored badge (Draft=gray, Generating=blue animated, Published=green, Failed=red, Archived=muted)
- Actions: View, Edit name, Publish, Delete, Export
- **"Generate New Timetable"** prominent button:
  1. Opens dialog: enter name, option to run preflight first
  2. If preflight: show results in dialog, allow proceeding or canceling
  3. On generate: navigate to timetable view page showing generation progress
- **Preflight button:** Runs preflight validation, shows results in a slide-over panel

### 5.11 Timetable View Page (/timetables/[id])

This is the **core experience** of the application.

**Header:**

- Timetable name + status badge
- Quality score display (circular gauge or progress ring, color-coded: green > 80, yellow 60-80, red < 60)
- View switcher: Group View | Teacher View | Master View (segmented control / tabs)
- Selector for specific group/teacher (dropdown, changes based on active view)
- Action buttons: Publish, Export, Print, View Report
- If status is "Generating": show progress indicator with status text, auto-refresh every 2 seconds

**Group View (default):**

- Grid layout: columns = teaching days (Mon, Tue, Wed...), rows = time slots (Period 1, Period 2, Break, Period 3...)
- Break rows shown as thin, gray, non-interactive dividers
- Each lesson cell shows:
  - Subject name (bold)
  - Teacher name (smaller text below)
  - Room name (if special room, even smaller or as icon)
  - Background color: subject color from palette
  - Double period: cell spans two rows with a subtle visual merge (slightly rounded, no border between the two slots)
  - Combined lesson indicator: small badge or icon showing "5A+5B"
- Empty slots: light gray background, dashed border
- Hover: subtle elevation/shadow
- Click (if Draft): opens edit popover (see 5.12)
- Selector: dropdown of all groups, organized by grade (optionally, grade tabs + group selector within)

**Teacher View:**

- Same grid layout as group view
- Each cell shows: subject name, group name(s), room
- Gaps in schedule highlighted with a subtle yellow/orange tint (connects to the report)
- Selector: dropdown of all teachers, sorted alphabetically

**Master View:**

- Day selector at top (single day at a time)
- Grid: columns = groups (grouped by grade with grade headers), rows = time slots
- Compact cells: subject abbreviation + teacher initials
- Color-coded by subject
- Hover to see full details in tooltip
- Grade filter: checkboxes or tab bar to show/hide grades
- Horizontal scroll with sticky first column (time slots)
- Useful for spotting: teacher conflicts, room conflicts, unbalanced loads

### 5.12 Timetable Cell Editing (Draft Only)

When a user clicks a cell in a Draft timetable:

- **Popover** anchored to the cell appears with:
  - Current assignment summary at top
  - **Subject** dropdown (filtered to subjects required for this grade that haven't met weekly quota)
  - **Teacher** dropdown (filtered to teachers qualified for selected subject at this grade level AND available at this time slot)
  - **Room** dropdown (only if subject requires special room, filtered to available rooms at this slot)
  - "Clear" button to unassign the slot
  - "Save" button → calls `PUT /entries/{entryId}`, handles 409 Conflict with inline error message explaining the constraint violation
  - "Cancel" button to dismiss
- Keyboard: Escape to close, Tab between fields, Enter to save
- After save: cell updates optimistically, reverts on error

### 5.13 Quality Report Page (/timetables/[id]/report)

- **Summary section:**
  - Overall quality score (large, color-coded gauge)
  - Counts by severity: X errors, Y warnings, Z info items
  - Three filter buttons to toggle each severity

- **Report items list:**
  - Grouped by category (TeacherSplit, GapInTeacherSchedule, etc.)
  - Each item: severity icon (red/yellow/blue), category badge, message text
  - Clickable: for items with a related entity, clicking navigates to the relevant entity or highlights the relevant cell(s) in the timetable view
  - Sortable by severity, category, entity

- **Actionable guidance:**
  - For each category, show a brief explanation of what it means and how to resolve it
  - "TeacherSplit" → "This group has two different teachers for [Subject]. Consider adjusting teacher availability or using double periods."
  - "GapInTeacherSchedule" → "Teacher [Name] has a free period between lessons on [Day]. This can be resolved by swapping adjacent lessons."

### 5.14 User Management Page (OrgAdmin Only)

- Table: name, email, role badge, auth method (email/OAuth), actions
- Invite user modal: email, display name, role selector
- Edit user role
- Delete user (with confirmation)

### 5.15 School Settings Page

- School name (editable)
- Default language selector (Bokmål / Nynorsk / English)
- Danger zone: delete all timetables, reset configuration (with confirmation dialogs)
- Re-run setup wizard button

### 5.16 My Schedule Page (/my-schedule)

- **Mobile-optimized**, responsive layout
- Shows the current teacher's weekly timetable in the published timetable
- Automatically detects the logged-in teacher by matching their email to a teacher record
- If no published timetable exists: friendly message "No published timetable yet"
- View: daily tab view on mobile (swipe between days), weekly grid on desktop
- Each lesson card: subject, group name(s), room, time
- No editing capabilities
- Export/print own schedule

---

## 6. Export & Print

### 6.1 PDF Export

- Generate PDF timetables using @react-pdf/renderer
- Available for: group view, teacher view, master view (one day per page)
- Layout: landscape A4, school name + timetable name header, grid matching the on-screen view
- Subject cells colored with subject colors
- Footer: "Generated by ClassForge" + date

### 6.2 Excel Export

- Export timetable data using SheetJS
- One worksheet per group OR one worksheet per teacher (user choice)
- Columns: Day, Period, Start Time, End Time, Subject, Teacher, Room
- Cell coloring matching subject colors

### 6.3 Print

- Print-optimized CSS media queries for direct browser printing
- Same layouts as PDF but using CSS @media print
- "Print" button triggers browser print dialog via react-to-print

### 6.4 Batch Export

- From timetable list: select multiple timetables or "Export all groups" / "Export all teachers"
- Generates a single PDF with page breaks between each group/teacher
- Or a single Excel file with one sheet per group/teacher

---

## 7. State Management

### 7.1 Server State (TanStack Query)

All API data managed through TanStack Query with these conventions:

```
Query keys:
  ["grades"]                           — all grades
  ["grades", gradeId, "groups"]        — groups for a grade
  ["subjects"]                         — all subjects
  ["teachers"]                         — all teachers
  ["teachers", teacherId]              — single teacher detail
  ["teachers", teacherId, "qualifications"]
  ["teachers", teacherId, "day-config"]
  ["teachers", teacherId, "blocked-slots"]
  ["teaching-days"]                    — all teaching days
  ["teaching-days", dayId, "time-slots"]
  ["timetables"]                       — all timetables
  ["timetables", timetableId]          — single timetable
  ["timetables", timetableId, "entries"]
  ["timetables", timetableId, "report"]
  ["timetables", timetableId, "by-group", groupId]
  ["timetables", timetableId, "by-teacher", teacherId]
```

- Stale time: 30 seconds for most queries, 5 seconds for timetable status during generation
- Optimistic updates for cell editing
- Invalidation: on mutation success, invalidate related queries
- Polling: timetable status polled every 2s while status === "Generating"

### 7.2 Client State (Zustand)

```
authStore:
  - accessToken, refreshToken, user profile
  - login(), logout(), refresh() actions

uiStore:
  - sidebarCollapsed: boolean
  - locale: "nb" | "nn" | "en"
  - timetableView: "group" | "teacher" | "master"
  - selectedGroupId, selectedTeacherId, selectedDayOfWeek (for master view)

setupWizardStore:
  - currentStep: number
  - completedSteps: Set<number>
  - templateSelection
```

---

## 8. API Client

### 8.1 Type Generation

Generate TypeScript types from `swagger.json` at build time:

```bash
npx openapi-typescript swagger.json -o src/lib/api/schema.ts
```

### 8.2 Client Setup

Use `openapi-fetch` for type-safe API calls:

```typescript
import createClient from "openapi-fetch";
import type { paths } from "./schema";

export const api = createClient<paths>({
  baseUrl: process.env.NEXT_PUBLIC_API_URL,
});
```

### 8.3 Auth Interceptor

- Attach `Authorization: Bearer {token}` header to all requests
- On 401: attempt token refresh, retry original request, redirect to login if refresh fails
- On 403: show "Insufficient permissions" toast

### 8.4 Error Handling

- API errors return `ProblemDetails` or `HttpValidationProblemDetails`
- Parse error responses and show:
  - Validation errors: inline on form fields
  - Conflict errors (409): inline message near the relevant action
  - Server errors (500): generic toast with "Something went wrong" + retry option
  - Network errors: "Connection lost" banner at top of screen

---

## 9. Responsive Design Strategy

### 9.1 Desktop-First

All planning and management features are designed for desktop (1024px+ viewport):

- Sidebar navigation always visible (collapsible)
- Full grid layouts for timetables
- Multi-column forms
- Drag-and-drop interactions

### 9.2 Mobile-Friendly (Viewer Role)

The `/my-schedule` page and timetable view page (read-only mode) are responsive:

- Breakpoints: mobile (< 768px), tablet (768-1023px), desktop (1024px+)
- Mobile: bottom tab navigation instead of sidebar, daily view (one day at a time with swipe/tabs), stacked card layout for lessons
- Tablet: sidebar becomes a hamburger/overlay, weekly grid with horizontal scroll

### 9.3 Minimum Viewport

- Admin features: 1024px minimum (show "Please use a larger screen" message below this)
- Viewer features: 320px minimum

---

## 10. Performance Requirements

- **Initial page load (LCP):** < 2 seconds
- **Navigation between pages:** < 500ms (client-side with prefetching)
- **Timetable grid render:** < 1 second for a school with 30 groups
- **API round-trip + UI update:** < 300ms perceived (optimistic updates)
- **Bundle size:** < 200KB initial JS (code-split aggressively by route)
- Use Next.js `loading.tsx` skeletons for every page
- Lazy-load heavy components: timetable grid, PDF renderer, Excel export

---

## 11. Project Structure

```
src/
  app/
    [locale]/
      (auth)/
        login/page.tsx
        register/page.tsx
      (app)/
        layout.tsx                    — authenticated layout with sidebar
        dashboard/page.tsx
        grades/page.tsx
        subjects/page.tsx
        rooms/page.tsx
        teachers/
          page.tsx
          [id]/page.tsx
        time-structure/page.tsx
        timetables/
          page.tsx
          [id]/
            page.tsx
            report/page.tsx
        users/page.tsx
        settings/page.tsx
        my-schedule/page.tsx
      setup/
        page.tsx                      — setup wizard
      layout.tsx                      — root locale layout
    layout.tsx                        — root layout (font, providers)
    not-found.tsx
  components/
    ui/                               — shadcn/ui components
    layout/
      sidebar.tsx
      header.tsx
      mobile-nav.tsx
    auth/
      login-form.tsx
      register-form.tsx
      oauth-buttons.tsx
    setup/
      wizard-shell.tsx
      step-*.tsx                      — one component per wizard step
    grades/
      grade-card.tsx
      group-list.tsx
    subjects/
      subject-table.tsx
      subject-form.tsx
      color-picker.tsx
    teachers/
      teacher-table.tsx
      teacher-detail.tsx
      qualification-form.tsx
      availability-grid.tsx
      blocked-slot-grid.tsx
    time-structure/
      day-toggle.tsx
      period-builder.tsx
      grade-day-matrix.tsx
    timetable/
      timetable-grid.tsx              — the core grid component
      timetable-cell.tsx
      cell-edit-popover.tsx
      view-switcher.tsx
      group-selector.tsx
      teacher-selector.tsx
      master-view.tsx
      generation-progress.tsx
      quality-gauge.tsx
    report/
      report-summary.tsx
      report-item-list.tsx
    export/
      pdf-timetable.tsx
      excel-export.ts
      print-layout.tsx
  lib/
    api/
      schema.ts                       — auto-generated from swagger.json
      client.ts                       — openapi-fetch client
      hooks/                          — TanStack Query hooks per domain
        use-grades.ts
        use-subjects.ts
        use-teachers.ts
        use-timetables.ts
        use-auth.ts
        ...
    stores/
      auth-store.ts
      ui-store.ts
      wizard-store.ts
    i18n/
      config.ts
      request.ts
    utils/
      date.ts
      color.ts
      timetable-helpers.ts
  messages/
    nb/
      common.json
      auth.json
      setup.json
      grades.json
      subjects.json
      rooms.json
      teachers.json
      timeStructure.json
      timetable.json
      users.json
      settings.json
    nn/
      ... (same structure)
    en/
      ... (same structure)
  styles/
    globals.css                       — Tailwind imports, CSS custom properties
```

---

## 12. Implementation Order (Suggested Milestones)

1. **Project scaffolding:** Next.js 16 + TypeScript + Tailwind 4 + shadcn/ui + pnpm setup. Configure next-intl, generate API types from swagger.json, set up openapi-fetch client, Zustand stores, TanStack Query provider. Configure Nunito font.

2. **Design system foundation:** Tailwind theme with brand colors, subject palette CSS variables, shadcn/ui component customization (button variants, badge variants), sidebar layout shell with responsive detection.

3. **Authentication:** Login page, registration page, JWT handling, auth store, route middleware, refresh token logic. OAuth button stubs (functional when backend OAuth is configured).

4. **Setup wizard:** All 8 steps with template pre-population for Norwegian school types. Step navigation, auto-save, skip/resume capability.

5. **CRUD pages — Academic structure:** Grades & groups, subjects (with color picker), rooms. Standard table/form patterns.

6. **CRUD pages — Time structure:** Teaching days, period builder, grade day config matrix.

7. **CRUD pages — Teachers:** Teacher list, detail page with three tabs (qualifications, availability, blocked slots). Availability and blocked-slot grids.

8. **Timetable generation:** Preflight validation UI, generation trigger with progress polling, generation progress screen.

9. **Timetable views:** Group view, teacher view, master view. View switcher, selectors, timetable grid component, cell rendering with subject colors and double period merging.

10. **Timetable editing:** Cell edit popover for Draft timetables, optimistic updates, constraint violation display.

11. **Quality report:** Report page with summary gauge, categorized issue list, navigation to related entities.

12. **Export & print:** PDF export, Excel export, print layouts, batch export.

13. **User management:** User list, invite, role editing, delete.

14. **My Schedule (mobile):** Responsive teacher view, daily tab navigation, mobile-optimized layout.

15. **Polish:** Loading skeletons, empty states, error boundaries, toast notifications, keyboard shortcuts, accessibility audit.

---

## 13. Norwegian School Templates

### 13.1 Barneskole (Primary School, Grades 1–7)

**Grades:** 1. klasse, 2. klasse, 3. klasse, 4. klasse, 5. klasse, 6. klasse, 7. klasse

**Default subjects (example, adjustable):**
Norsk, Matematikk, Engelsk, Naturfag, Samfunnsfag, KRLE, Musikk, Kunst og håndverk, Kroppsøving, Mat og helse, Norsk fordypning/Samisk

**Typical schedule:** Mon–Fri, 5–6 periods/day for lower grades, 6 periods/day for upper grades. Period length: 45 min. Two periods, 10 min break, two periods, 30 min lunch, one-two periods.

### 13.2 Ungdomsskole (Lower Secondary, Grades 8–10)

**Grades:** 8. klasse, 9. klasse, 10. klasse

**Default subjects:** Norsk, Matematikk, Engelsk, Naturfag, Samfunnsfag, KRLE, Musikk, Kunst og håndverk, Kroppsøving, Mat og helse, Fremmedspråk/Språklig fordypning/Arbeidslivsfag, Valgfag, Utdanningsvalg

**Typical schedule:** Mon–Fri, 6–7 periods/day. Period length: 45 or 60 min.

### 13.3 Combined (1–10)

Combines both templates above.

### 13.4 Videregående (Upper Secondary, VG1–VG3)

**Grades:** VG1, VG2, VG3

**Default subjects:** Highly variable by study program. Provide only common core: Norsk, Matematikk, Engelsk, Naturfag, Samfunnsfag, Kroppsøving, and leave the rest for manual configuration.

**Typical schedule:** Mon–Fri, 7–8 periods/day.

---

## 14. Accessibility

- WCAG 2.1 Level AA compliance target
- All interactive elements keyboard-accessible
- ARIA labels on all icon-only buttons
- Color contrast ratio ≥ 4.5:1 for text, ≥ 3:1 for large text
- Focus visible indicators (2px Green ring)
- Screen reader announcements for: toast notifications, form validation errors, timetable cell changes
- Subject colors are supplementary — always include text labels (don't rely on color alone)
- Skip-to-content link
- Reduced motion: respect `prefers-reduced-motion` media query

---

## 15. Testing Strategy

- **Unit tests:** Vitest for utility functions, timetable helpers, state stores
- **Component tests:** Vitest + Testing Library for key interactive components (timetable grid, cell edit popover, wizard steps)
- **E2E tests:** Playwright for critical flows: login → setup wizard → generate timetable → view → export
- **Visual regression:** Optional, using Playwright screenshots
- **Accessibility:** axe-core integration in component tests
