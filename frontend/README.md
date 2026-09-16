# Purchase Bill — Frontend (Angular + Angular Material)

Angular SPA for the assignment: a login page (Task 1) and, next, the Purchase Bill form
(Task 2). Built with Angular 22 (standalone components, signals, zoneless change detection)
and Angular Material.

## Status

- ✅ **Login page** — matches the provided screenshot, validates input, calls the backend,
  stores the session, and redirects to a placeholder `/dashboard` route.
- ✅ **Route protection** — `/dashboard` (standing in for the Purchase Bill page until Task 2
  is built) is behind `authGuard` and redirects to `/login` without a valid session.
- ⬜ **Purchase Bill form** — not built yet.

## Architecture

```
src/app/
  core/
    models/         LoginRequest/LoginResponse DTOs mirroring the backend, ApiProblemDetails helper
    services/       AuthService - session state (signals) + the login HTTP call
    interceptors/   auth.interceptor (attaches the JWT), error.interceptor (401 -> logout + redirect)
    guards/          authGuard - protects routes behind a valid session
  features/
    auth/login/     the login page
    dashboard/       placeholder landing page after login (Task 2 will replace this)
```

`AuthService` keeps the JWT in `sessionStorage` (cleared when the tab closes) and exposes
`isAuthenticated`/`username` as signals, so the guard and any component can react without
polling.

## Prerequisites

- Node.js 22.22+ or 24.15+ (the Angular CLI's minimum — see `package.json` `engines` via
  `@angular/cli`). This repo was built against Node 24.21.0 LTS.
- The backend running at `http://localhost:5029` (see [`../backend/README.md`](../backend/README.md)).

## Setup

```bash
npm install
npm start          # ng serve, http://localhost:4200
```

`src/environments/environment.development.ts` points `apiUrl` at
`http://localhost:5029/api`; `environment.ts` (production) uses a relative `/api` so it can be
reverse-proxied alongside the built API.

## Testing

```bash
npm test
```

Runs the Vitest-based unit tests: `AuthService` (login/logout, session persistence, expiry),
and the `Login` component (validation messages, successful login + redirect, and surfacing the
backend's error message on failure).

## Notes

- The backend's shared Enhanzer demo account can occasionally answer with "the authentication
  service is busy right now" if hit right after another recent login attempt — see the backend
  README for why. That message comes straight from the API and is shown as-is on the login form.
