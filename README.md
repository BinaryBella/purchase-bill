# Purchase Bill

Full Stack Developer (Angular & .NET Core) assignment: a login screen, a Purchase Bill form and
a welcome dashboard — built against the provided UI designs.

- **Task 1 — Login:** authenticates against Enhanzer's external POS API, saves the returned
  `User_Locations` into SQL Server, and protects the Purchase Bill page behind that login.
- **Task 2 — Purchase Bill:** an Item autocomplete, a Batch dropdown sourced from the saved
  locations, computed Margin / Total Cost / Total Selling per row, an items table, and an Item
  Summary panel (Total Items / Total Quantity).
- **Task 3 — Welcome dashboard:** the landing page after login, with three widgets fed by new
  read endpoints — the latest 5 purchase orders, the oldest 10 order items, and a donut chart of
  quantity by item name. Saved bills now get a readable purchase order number (`PO-000012`).

## Tech stack

| Layer    | Tech |
|----------|------|
| Frontend | Angular 22 (standalone components, signals, zoneless), Angular Material, TypeScript |
| Backend  | ASP.NET Core 8, C#, Entity Framework Core |
| Database | SQL Server |

## Repository structure

```
purchase-bill/
├── backend/    ASP.NET Core 8 Web API (Clean Architecture)
├── frontend/   Angular + Angular Material SPA
└── README.md   this file
```

---

## Backend

Clean-architecture style, three projects plus a test project:

```
backend/
  src/
    PurchaseBill.Api             ASP.NET Core Web API - controllers, JWT auth, Swagger, middleware
    PurchaseBill.Application     Entities, DTOs, services (business logic), FluentValidation validators
    PurchaseBill.Infrastructure  EF Core (SQL Server), the Enhanzer HTTP client, JWT issuing
  tests/
    PurchaseBill.Tests           xUnit + Moq + EF Core InMemory
  database/
    PurchaseBillDb.sql           Idempotent SQL script generated from the EF Core migrations
                                 (InitialCreate + AddPoNumberAndDashboardIndexes)
```

`PurchaseBill.Application` only depends on its own interfaces (`IApplicationDbContext`,
`IEnhanzerAuthClient`, `IJwtTokenGenerator`) — it has no reference to EF Core's SQL Server
provider or to `HttpClient` directly, so the business logic (login flow, bill calculations) is
tested in isolation from both the database and the external API.

### How login works

The SPA never talks to Enhanzer directly — only this API does:

1. `POST /api/auth/login` receives `{ email, password }`.
2. The API calls `https://ez-staging-api.azurewebsites.net/api/External_Api/POS_Api/Invoke`
   with `API_Action=GetLoginData`, using the email for both `Company_Code` and `Username`.
3. Enhanzer's response is one of three shapes (confirmed against staging):
   - success: `Status_Code 200`, `Response_Body[0].User_Locations` populated.
   - wrong password: `Status_Code 200`, `Response_Body[0].Doc_Msg = "Invalid Login Details"`.
   - unknown company/user: `Status_Code 401`, `Response_Body = null`.
4. On success, `User_Locations` is upserted into the `Location_Details` table (by
   `Location_Code`, so logging in again doesn't create duplicates).
5. The API issues its own short-lived JWT, which the Angular app then sends as
   `Authorization: Bearer <token>` on every subsequent call.

> **Known staging quirk:** the demo credentials (`info@enhanzer.com` / `Welcome#5`) are shared
> across everyone building this assignment. Firing logins back-to-back against that account
> sometimes gets throttled by the staging API, which answers HTTP 200 with an empty body
> (`Status_Code: 0, Response_Body: null`) instead of a real error. The API detects this shape
> and returns a clear "the authentication service is busy, try again" message rather than a
> misleading generic failure. A normal, one-click-at-a-time login is not affected.

### Backend prerequisites

- .NET 8 SDK (pinned via `global.json`)
- SQL Server reachable from your machine (a local instance, LocalDB, or a Docker container)

### Backend setup

**1. Configure secrets** (never committed — kept out of `appsettings.json` on purpose):

```bash
cd backend/src/PurchaseBill.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=PurchaseBillDb;User Id=sa;Password=<your-sa-password>;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:SigningKey" "<any long random string, 32+ chars>"
cd ../../..
```

`Enhanzer:BaseUrl` and the `Jwt:Issuer`/`Audience`/`ExpiryMinutes` values are already set in
`appsettings.json` (they aren't secrets).

**2. Apply the database migration:**

```bash
dotnet tool install -g dotnet-ef   # once, if you don't already have it
dotnet ef database update --project backend/src/PurchaseBill.Infrastructure --startup-project backend/src/PurchaseBill.Api
```

Alternatively, run `backend/database/PurchaseBillDb.sql` directly against SQL Server — it's the
same migration, exported as an idempotent script (safe to re-run).

**3. Run the API:**

```bash
dotnet run --project backend/src/PurchaseBill.Api
```

The API listens on `http://localhost:5029` by default (see `launchSettings.json`).

**Swagger UI: [http://localhost:5029/](http://localhost:5029/)** — served at the root, in every
environment (not just Development), since this is an assignment deliverable meant to be
explored by a reviewer. Each endpoint's description comes from the `///` doc comments already on
the controllers/DTOs. Use "Authorize" with the token returned from `/api/auth/login` to call the
protected endpoints from there too.

### Backend endpoints

| Method | Route                 | Auth | Purpose |
|--------|-----------------------|------|---------|
| POST   | `/api/auth/login`     | No   | Task 1 login; saves `Location_Details`; returns a JWT |
| GET    | `/api/items`          | Yes  | Fixed item catalog for the "Item" autocomplete |
| GET    | `/api/locations`      | Yes  | Saved locations for the "Batch" dropdown |
| POST   | `/api/purchase-bills` | Yes  | Task 2 submit; persists the bill + its line items, returns the `poNumber` |
| GET    | `/api/purchase-bills?page=&pageSize=` | Yes | Saved bills, newest first (paged, `pageSize` capped at 100) |
| GET    | `/api/purchase-bills/{id}` | Yes | One saved bill with its line items (404 if unknown) |
| GET    | `/api/dashboard/latest-orders` | Yes | Widget 01: the 5 newest bills — `id`, `poNumber`, `netAmount`, `itemCount`, `createdAt` |
| GET    | `/api/dashboard/oldest-items` | Yes | Widget 02: the 10 oldest line items — `purchaseOrderId`, `poNumber`, `itemName`, `quantity` |
| GET    | `/api/dashboard/items-by-quantity` | Yes | Widget 03: quantity summed per item name, with each item's percent of the total |

`latest-orders` and `oldest-items` take no filters: they always cover every bill, newest / oldest
first. Only `items-by-quantity` accepts an optional `?range=Today|Last7Days|Last30Days|All`
(default `Today`; evaluated in UTC, matching how bills are timestamped). **Net Amount**
is the bill's `Total_Cost` — the amount after discounts, i.e. what the buyer actually pays —
rather than `Total_Selling`, which is the resale value.

### Purchase order numbers

Each bill gets a unique `Po_Number` (`PO-` + the bill's Id, zero-padded to 6 digits). The Id only
exists once the row is inserted, so the service saves once with a temporary placeholder and then
a second time with the real number. The `AddPoNumberAndDashboardIndexes` migration also
backfills bills saved before it existed, and adds indexes on `Purchase_Bill.Created_At` and
`Purchase_Bill_Item.Item_Name` for the dashboard's date sorting and grouping.

### Running backend tests

```bash
dotnet test backend
```

Tests covering the `Total Cost` / `Total Selling` / `Margin` formulas (including the brief's
worked example: Cost 100, Price 150, Qty 5, Discount 20% → Total Cost 400, Total Selling 750),
the login service against all of Enhanzer's response shapes above, the purchase bill service's
totals/summary logic and PO numbering, paging and lookup, the dashboard queries (ordering, the
5/10 limits, all-time coverage, grouping and percentages, date ranges, empty data), the FluentValidation rules, and
a check that the EF Core model snapshot matches the entities (so a model change cannot ship
without its migration).

### Regenerating the SQL script

After adding a new EF Core migration:

```bash
dotnet ef migrations script \
  --project backend/src/PurchaseBill.Infrastructure \
  --startup-project backend/src/PurchaseBill.Api \
  --idempotent \
  --output backend/database/PurchaseBillDb.sql
```

---

## Frontend

Angular SPA: a login page (Task 1), the Purchase Bill form (Task 2) and the welcome dashboard
(Task 3), built with Angular 22
(standalone components, signals, zoneless change detection) and Angular Material.

- ✅ **Login page** — matches the provided screenshot, validates input, calls the backend,
  stores the session, and redirects to `/dashboard`.
- ✅ **Route protection** — `/dashboard` and `/purchase-bill` are behind `authGuard` and redirect
  to `/login` without a valid session.
- ✅ **Welcome dashboard** — replicates the provided design: an app shell (gradient top bar and
  dark module sidebar) around three widget cards, each with its own date-range dropdown.
- ✅ **Purchase Bill form** — Item autocomplete, Batch dropdown (from the locations saved at
  login), live Margin/Total Cost/Total Selling, an items grid with remove, an Item Summary
  panel, and Save (persists the whole bill to the backend in one call and shows its PO number).

### Welcome dashboard

| Widget | Shows | Date filter |
|--------|-------|-------------|
| Latest Purchase Orders (table) | The 5 most recently added orders: Order No, Net Amount, No. of Items | None |
| Oldest Order Items (list) | The 10 oldest line items: PO number, Item Name, Quantity | None |
| Quantity by Item (donut) | Total quantity grouped by item name, with the grand total in the centre and a colour-keyed legend | Dropdown (Today / Last 7 days / Last 30 days / All time), default All time |

The table and list always show the latest 5 / oldest 10 across all orders, so they have no date
dropdown. The donut keeps one, and its dropdown refetches only that widget. Every widget handles
its own loading, error (with a Retry button) and empty states, so one failing request does not
blank the page. The donut is plain SVG — no charting dependency.

Notes on fidelity to the mockup: the design's Bank Accounts / Expenses / Delayed Orders data
does not exist in this app, so the widgets show the fields the brief specifies. Sidebar modules
other than Dashboard and Procurement (which opens the Purchase Bill form), plus the bell,
settings and help icons, are visual placeholders only and are disabled.

### Frontend architecture

```
frontend/src/app/
  core/
    models/         DTOs mirroring the backend (auth, purchase bill, dashboard), ApiProblemDetails helper
    services/       AuthService (session signals + login call), PurchaseBillService (items/locations/create),
                    DashboardService (the three widget endpoints)
    layout/         app-shell - top bar + sidebar wrapping every authenticated page (also hosts Log out)
    interceptors/   auth.interceptor (attaches the JWT), error.interceptor (401 -> logout + redirect)
    guards/         authGuard - protects routes behind a valid session
    utils/          purchase-bill-calculator - mirrors the backend's Margin/Total Cost/Total Selling formulas
  features/
    auth/login/               the login page
    dashboard/
      dashboard-page/         3-column responsive grid (2 columns, then 1, on narrower screens)
      widget-loader.ts        shared range/refetch/loading-error state for a widget
      components/
        widget-card/          card chrome: title, optional range dropdown, loading/error/empty states
        latest-orders-table/  widget 01
        oldest-items-list/    widget 02
        items-donut/          widget 03 (SVG arcs + legend)
    purchase-bill/
      purchase-bill-page/     container: loads the catalog, owns the row list, header actions (Save/Close)
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

### Frontend prerequisites

- Node.js 22.22+ or 24.15+ (the Angular CLI's minimum — see `package.json` `engines` via
  `@angular/cli`). This repo was built against Node 24.21.0 LTS.
- The backend running at `http://localhost:5029` (see the Backend section above).

### Frontend setup

```bash
cd frontend
npm install
npm start   # ng serve, http://localhost:4200
```

`src/environments/environment.development.ts` points `apiUrl` at
`http://localhost:5029/api`; `environment.ts` (production) uses a relative `/api` so it can be
reverse-proxied alongside the built API.

### Running frontend tests

```bash
cd frontend
npm test
```

50 Vitest tests: `AuthService`, `PurchaseBillService` and `DashboardService` (HTTP calls via
`HttpClientTestingModule`), the calculation utility (including the brief's worked example),
`Login`, the Purchase Bill components (`ItemEntryForm`'s live calculations/validation/catalog
check, `ItemTable`, `ItemSummary`, and `PurchaseBillPage`'s load/add/remove/save/close flows), and
the dashboard widgets (rendering, no dropdown on the table/list, donut range changes, empty and error/retry states,
donut arc maths).

Manually verified end to end against the real backend and SQL Server too: logging in, adding
rows, watching the Item Summary and live calculations update, removing a row, and saving -
confirmed the persisted bill's totals in the database matched what the UI showed.

### Frontend notes

- The backend's shared Enhanzer demo account can occasionally answer with "the authentication
  service is busy right now" if hit right after another recent login attempt — see "How login
  works" above for why. That message comes straight from the API and is shown as-is on the
  login form.

---

## Deliverables checklist

- [x] GitHub repository, with a meaningful commit history
- [x] SQL Server database script — [backend/database/PurchaseBillDb.sql](backend/database/PurchaseBillDb.sql)
- [x] Project README with setup instructions (this file)
- [x] Frontend (Angular + Angular Material) — login page, Purchase Bill form and welcome dashboard done
- [ ] 5–10 minute screen recording
- [ ] Completed submission form
