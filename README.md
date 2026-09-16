# Purchase Bill

Full Stack Developer (Angular & .NET Core) assignment: a 2-page app — a login screen and a
Purchase Bill form — built against the provided UI designs.

- **Task 1 — Login:** authenticates against Enhanzer's external POS API, saves the returned
  `User_Locations` into SQL Server, and protects the Purchase Bill page behind that login.
- **Task 2 — Purchase Bill:** an Item autocomplete, a Batch dropdown sourced from the saved
  locations, computed Margin / Total Cost / Total Selling per row, an items table, and an Item
  Summary panel (Total Items / Total Quantity).

## Tech stack

| Layer    | Tech |
|----------|------|
| Frontend | Angular (latest), Angular Material, TypeScript |
| Backend  | ASP.NET Core 8, C#, Entity Framework Core |
| Database | SQL Server |

## Repository structure

```
purchase-bill/
├── backend/    ASP.NET Core 8 Web API (Clean Architecture) - see backend/README.md
├── frontend/   Angular + Angular Material SPA               - see frontend/README.md
└── README.md   this file
```

## Backend

Full architecture notes and setup steps are in **[backend/README.md](backend/README.md)**.
Quick start:

```bash
# 1. Configure secrets (once) - see backend/README.md for the exact commands
cd backend/src/PurchaseBill.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=PurchaseBillDb;User Id=sa;Password=<your-sa-password>;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:SigningKey" "<any long random string, 32+ chars>"
cd ../../..

# 2. Create the database
dotnet ef database update --project backend/src/PurchaseBill.Infrastructure --startup-project backend/src/PurchaseBill.Api

# 3. Run the API
dotnet run --project backend/src/PurchaseBill.Api
```

The API listens on `http://localhost:5029` by default (see `launchSettings.json`), with Swagger
UI at `/swagger`. Run the test suite with `dotnet test backend`.

### Backend endpoints

| Method | Route                 | Auth | Purpose |
|--------|-----------------------|------|---------|
| POST   | `/api/auth/login`     | No   | Task 1 login; saves `Location_Details`; returns a JWT |
| GET    | `/api/items`          | Yes  | Fixed item catalog for the "Item" autocomplete |
| GET    | `/api/locations`      | Yes  | Saved locations for the "Batch" dropdown |
| POST   | `/api/purchase-bills` | Yes  | Task 2 submit; persists the bill + its line items |

## Frontend

Full details are in **[frontend/README.md](frontend/README.md)**. Quick start (needs Node
22.22+ or 24.15+, and the backend running at `http://localhost:5029`):

```bash
cd frontend
npm install
npm start   # http://localhost:4200
```

Both pages are built: the **login page** (Task 1), matching the provided screenshot, and the
**Purchase Bill form** (Task 2) - Item autocomplete, Batch dropdown, live Margin/Total
Cost/Total Selling, an items grid, an Item Summary panel, and Save, which persists the whole
bill to the backend. `/purchase-bill` is behind a route guard reachable only after login.

## Deliverables checklist

- [x] GitHub repository, with a meaningful commit history
- [x] SQL Server database script — [backend/database/PurchaseBillDb.sql](backend/database/PurchaseBillDb.sql)
- [x] Backend README with setup instructions
- [x] Frontend (Angular + Angular Material) — login page and Purchase Bill form both done
- [x] Frontend README
- [ ] 5–10 minute screen recording
- [ ] Completed submission form
