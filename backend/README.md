# Purchase Bill — Backend (ASP.NET Core 8)

Backend for the Full Stack Developer assignment: a login that authenticates against the
external Enhanzer POS API and a Purchase Bill API that stores items against locations pulled
from that login.

## Architecture

Clean-architecture style, three projects plus a test project:

```
src/
  PurchaseBill.Api             ASP.NET Core Web API - controllers, JWT auth, Swagger, middleware
  PurchaseBill.Application     Entities, DTOs, services (business logic), FluentValidation validators
  PurchaseBill.Infrastructure  EF Core (SQL Server), the Enhanzer HTTP client, JWT issuing
tests/
  PurchaseBill.Tests           xUnit + Moq + EF Core InMemory
database/
  PurchaseBillDb.sql           Idempotent SQL script generated from the EF Core migrations
```

`PurchaseBill.Application` only depends on its own interfaces (`IApplicationDbContext`,
`IEnhanzerAuthClient`, `IJwtTokenGenerator`) — it has no reference to EF Core's SQL Server
provider or to `HttpClient` directly, so the business logic (login flow, bill calculations) is
tested in isolation from both the database and the external API.

## How login works

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

## Prerequisites

- .NET 8 SDK (pinned via `global.json`)
- SQL Server reachable from your machine (a local instance, LocalDB, or a Docker container)

## Setup

**1. Configure secrets** (never committed — kept out of `appsettings.json` on purpose):

```bash
cd src/PurchaseBill.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=PurchaseBillDb;User Id=sa;Password=<your-sa-password>;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:SigningKey" "<any long random string, 32+ chars>"
```

`Enhanzer:BaseUrl` and the `Jwt:Issuer`/`Audience`/`ExpiryMinutes` values are already set in
`appsettings.json` (they aren't secrets).

**2. Apply the database migration:**

```bash
dotnet tool install -g dotnet-ef   # once, if you don't already have it
dotnet ef database update \
  --project src/PurchaseBill.Infrastructure \
  --startup-project src/PurchaseBill.Api
```

Alternatively, run `database/PurchaseBillDb.sql` directly against SQL Server — it's the same
migration, exported as an idempotent script (safe to re-run).

**3. Run the API:**

```bash
dotnet run --project src/PurchaseBill.Api
```

Swagger UI is at `https://localhost:<port>/swagger` in Development. Use "Authorize" with the
token returned from `/api/auth/login` to call the protected endpoints from there too.

## Endpoints

| Method | Route                | Auth | Purpose |
|--------|----------------------|------|---------|
| POST   | `/api/auth/login`    | No   | Task 1 login; saves `Location_Details`; returns a JWT |
| GET    | `/api/items`         | Yes  | Fixed item catalog for the "Item" autocomplete |
| GET    | `/api/locations`     | Yes  | Saved locations for the "Batch" dropdown |
| POST   | `/api/purchase-bills`| Yes  | Task 2 submit; persists the bill + its line items |

## Running tests

```bash
dotnet test
```

27 tests covering the `Total Cost` / `Total Selling` / `Margin` formulas (including the brief's
worked example: Cost 100, Price 150, Qty 5, Discount 20% → Total Cost 400, Total Selling 750),
the login service against all of Enhanzer's response shapes above, the purchase bill service's
totals/summary logic, and the FluentValidation rules.

## Regenerating the SQL script

After adding a new EF Core migration:

```bash
dotnet ef migrations script \
  --project src/PurchaseBill.Infrastructure \
  --startup-project src/PurchaseBill.Api \
  --idempotent \
  --output database/PurchaseBillDb.sql
```
