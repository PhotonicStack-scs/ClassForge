# ClassForge Web

The frontend for **ClassForge** — a smart timetable planner built for Norwegian schools. Admins configure school structure and generate optimized weekly timetables; teachers view their personal schedules.

Built with Next.js 16, TypeScript, Tailwind CSS, and shadcn/ui. Designed to deploy on Azure Static Web Apps (Hybrid Next.js mode).

---

## Features

- **Multi-tenant** — each school is an isolated tenant
- **Setup wizard** — 8-step onboarding; selecting a built-in template (barneskole, ungdomsskole, combined, videregående) automatically seeds grades and subjects via bulk API calls so those steps can be skipped
- **Academic structure** — full CRUD for grades, subjects (with color coding), and rooms; each grade supports sub-groups (e.g. 5A, 5B) with inline add/rename/delete; all entities support inline editing
- **Time structure** — enable teaching days (Mon–Sun), build a period/break template, and fine-tune per-day slots via a weekly calendar grid
- **Teacher management** — qualifications, availability grids, blocked time slots
- **Timetable generation** — async generation with real-time progress polling
- **Timetable views** — group view, teacher view, with color-coded subject cells
- **Manual editing** — click any cell to reassign subject/teacher/room; hard-constraint validation on save
- **Quality report** — scored soft-constraint analysis grouped by severity
- **Export** — Excel (SheetJS), PDF (@react-pdf/renderer), browser print (react-to-print)
- **User management** — invite users, assign roles (OrgAdmin / ScheduleManager / Viewer)
- **My Schedule** — mobile-optimized teacher self-service view
- **Localization** — Norwegian Bokmål, Nynorsk, English

---

## Getting Started

### Prerequisites

- Node.js 20+
- npm
- The [ClassForge API](../ClassForge/) running on `http://localhost:5208`

### Install & Run

```bash
npm install
npm run dev
```

Open [http://localhost:3000](http://localhost:3000). You will be redirected to `/nb` (Norwegian Bokmål).

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `NEXT_PUBLIC_API_URL` | ClassForge API base URL | `http://localhost:5208/` |

A `.env.local` file is included with the dev default. For production, set `NEXT_PUBLIC_API_URL` in the Azure portal.

---

## Scripts

```bash
npm run dev      # Start development server (Turbopack)
npm run build    # Production build
npm start        # Start production server
npm run lint     # ESLint
```

Regenerate API types after updating the swagger spec:

```bash
npx openapi-typescript requirements/swagger.json -o src/lib/api/schema.ts
```

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | Next.js 16 (App Router, Hybrid SSR) |
| Language | TypeScript 5 |
| Styling | Tailwind CSS 4 + shadcn/ui |
| API client | openapi-fetch + openapi-typescript |
| State | Zustand |
| Data fetching | TanStack React Query v5 |
| Forms | react-hook-form + Zod |
| i18n | next-intl (nb / nn / en) |
| Tables | TanStack React Table v8 |
| Charts | Recharts |
| Dates | date-fns |
| Toasts | Sonner |
| Export | SheetJS (xlsx), @react-pdf/renderer, react-to-print |
| Animation | Framer Motion |
| Font | Nunito (Google Fonts) |
| Deployment | Azure Static Web Apps (Hybrid Next.js) |

---

## Project Structure

```
src/
├── app/
│   ├── [locale]/
│   │   ├── (auth)/          # Login, register
│   │   ├── (app)/           # Protected pages (sidebar layout)
│   │   │   ├── dashboard/
│   │   │   ├── grades/
│   │   │   ├── subjects/
│   │   │   ├── rooms/
│   │   │   ├── teachers/
│   │   │   ├── time-structure/
│   │   │   ├── timetables/
│   │   │   ├── users/
│   │   │   ├── my-schedule/
│   │   │   └── settings/
│   │   └── setup/           # Onboarding wizard
│   ├── globals.css          # Brand theme + Tailwind
│   └── not-found.tsx
├── components/
│   ├── layout/              # Sidebar, header, mobile nav
│   ├── auth/                # Login & register forms
│   ├── setup/               # Wizard steps
│   ├── timetable/           # Grid, cell, quality gauge, edit popover
│   ├── report/              # Quality report components
│   ├── export/              # Excel & print helpers
│   ├── providers/           # React Query, auth init, toast listener
│   └── ui/                  # shadcn/ui primitives
├── lib/
│   ├── api/
│   │   ├── schema.ts        # Generated OpenAPI types (do not edit)
│   │   ├── client.ts        # openapi-fetch + auth interceptor
│   │   └── hooks/           # Per-resource TanStack Query hooks
│   ├── stores/              # Zustand: auth, ui, wizard
│   └── utils/               # cn(), subject color helpers
├── i18n/                    # next-intl config & routing
├── messages/                # Translation files (nb / nn / en)
└── proxy.ts                 # Locale routing + auth cookie guard
```

---

## Authentication

- JWT-based. Tokens issued by the ClassForge API.
- **Access token** — kept in memory only (never localStorage).
- **Refresh token** — stored in localStorage; silently refreshed 60 s before expiry.
- **`cf_has_session` cookie** — set on login; read by `proxy.ts` to protect routes server-side.
- Roles: `OrgAdmin`, `ScheduleManager`, `Viewer`.

---

## Deployment (Azure Static Web Apps)

1. Set `NEXT_PUBLIC_API_URL` to your production API URL in the Azure portal.
2. Ensure the backend has CORS configured to allow the SWA domain.
3. `staticwebapp.config.json` is included at the project root — no additional SWA config needed.

---

## Related

- [ClassForge API](../ClassForge/) — ASP.NET Core backend (.NET 10, PostgreSQL)
- [Frontend spec](requirements/ClassForge_Frontend_Spec.md)
- [API reference](requirements/swagger.json)
