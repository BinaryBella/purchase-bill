# Purchase Bill — Frontend (Angular + Angular Material)

Angular SPA for the assignment: a login page (Task 1) and the Purchase Bill form (Task 2).
Built with Angular 22 (standalone components, signals, zoneless change detection) and Angular
Material.

## Status

- ✅ **Login page** — matches the provided screenshot, validates input, calls the backend,
  stores the session, and redirects to `/purchase-bill`.
- ✅ **Route protection** — `/purchase-bill` is behind `authGuard` and redirects to `/login`
  without a valid session.
- ✅ **Purchase Bill form** — Item autocomplete, Batch dropdown (from the locations saved at
  login), live Margin/Total Cost/Total Selling, an items grid with remove, an Item Summary
  panel, and Save (persists the whole bill to the backend in one call).

## Architecture

```
src/app/
  core/
    models/         DTOs mirroring the backend (auth, purchase bill), ApiProblemDetails helper
    services/       AuthService (session signals + login call), PurchaseBillService (items/locations/create)
    interceptors/   auth.interceptor (attaches the JWT), error.interceptor (401 -> logout + redirect)
    guards/         authGuard - protects routes behind a valid session
    utils/          purchase-bill-calculator - mirrors the backend's Margin/Total Cost/Total Selling formulas
  features/
    auth/login/               the login page
    purchase-bill/
      purchase-bill-page/     container: loads the catalog, owns the row list, header actions (Save/Logout)
      components/
        item-entry-form/      the Item/Batch/cost/qty/discount row + "Add"
        item-table/           the grid of added rows, with remove
        item-summary/         Total Items / Total Quantity panel
```

`AuthService` keeps the JWT in `sessionStorage` (cleared when the tab closes) and exposes
`isAuthenticated`/`username` as signals, so the guard and any component can react without
polling. `PurchaseBillPage` keeps the in-progress bill (`rows`) purely client-side - "Add" never
touches the backend, only "Save" does, matching the brief's Add-then-summarize flow with one
persist step at the end. The `Header`/`Notes`/`Documents`/`Accounts`/`Taxes` tabs from the
screenshot are present for visual fidelity but out of this assignment's functional scope, and
say so.

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

34 Vitest tests: `AuthService` and `PurchaseBillService` (HTTP calls via `HttpClientTestingModule`),
the calculation utility (including the brief's worked example), `Login`, and the Purchase Bill
components (`ItemEntryForm`'s live calculations/validation/catalog check, `ItemTable`,
`ItemSummary`, and `PurchaseBillPage`'s load/add/remove/save/logout flows).

Manually verified end to end against the real backend and SQL Server too: logging in, adding
rows, watching the Item Summary and live calculations update, removing a row, and saving -
confirmed the persisted bill's totals in the database matched what the UI showed.

## Notes

- The backend's shared Enhanzer demo account can occasionally answer with "the authentication
  service is busy right now" if hit right after another recent login attempt — see the backend
  README for why. That message comes straight from the API and is shown as-is on the login form.
