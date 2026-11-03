# Product Catalog API

A .NET 10 Web API for managing a product catalog with categories, authentication, refresh tokens, stock adjustments and reporting. It uses SQL Server (in Docker), JWT authentication, a stored procedure for concurrency-safe stock changes, and structured logging.

## Overview

Users can:

- register and log in
- use JWT bearer tokens for protected endpoints
- refresh and revoke refresh tokens
- manage products and categories
- adjust product stock safely under concurrent requests (with an audit trail)
- view inventory and category reports

## Tech Stack

- ASP.NET Core Web API on .NET 10
- Entity Framework Core + SQL Server 2022 (Docker)
- T-SQL stored procedure for stock adjustments
- JWT authentication, Argon2 password hashing
- Serilog, Swagger / OpenAPI
- xUnit; Testcontainers for integration tests against a real SQL Server
- Docker Compose with a one-shot migration bundle

## Project Structure

- `Program.cs` – startup, dependency injection, authentication, Swagger, health checks, admin seeding
- `Controllers/` – API endpoints for auth, products, categories and reports
- `Services/` – business logic and token helpers
- `Data/` – EF Core context
- `Models/` – entity classes (including `StockMovement`)
- `DTOs/`, `Interfaces/`, `Extensions/`
- `Migrations/` – EF Core migrations, including the `usp_AdjustStock` procedure and the price index
- `db/init-app-login.sql` – creates the limited database login used by the API
- `Dockerfile` – multi-stage: build, API runtime image, and the migrator image
- `docker-compose.yml` – `db` → `migrator` → `dbsetup` → `api`
- `docs/performance.md` – execution plans, logical reads and the index change
- `ProductCatalogApi.Tests/` – unit and integration tests

## Prerequisites

- .NET 10 SDK
- Docker Desktop (required to run the app **and** the integration tests)
- EF Core CLI tools, only for running without the migrator:

```bash
dotnet tool install --global dotnet-ef
```

## Configuration

All settings come from environment variables. Copy the template and fill it in:

```bash
cp .env.example .env        # Windows PowerShell: Copy-Item .env.example .env
```

| Variable | Purpose |
|---|---|
| `DB_PASSWORD` | SQL Server `sa` password (8+ chars: upper, lower, digit, symbol). Used by SQL Server, the migrator and the setup step. |
| `APP_DB_PASSWORD` | Password of the limited `catalog_app` login the API connects with. Must not contain a single quote (`'`). |
| `JWT_KEY` | JWT signing key, at least 32 characters. |
| `ADMIN_SEED_USERNAME` | Username of the first admin account, created at startup if no admin exists. |
| `ADMIN_SEED_PASSWORD` | Password of that admin account. |
| `ASPNETCORE_ENVIRONMENT` | Optional, defaults to `Development` (Swagger is only enabled in Development). |

`.env` is git-ignored. Never commit it. Compose refuses to start if a required variable is missing.

## Run with Docker Compose (recommended)

```bash
docker compose up --build
```

Services start in this order:

1. **db** – SQL Server 2022, with a healthcheck.
2. **migrator** – applies all EF Core migrations using the `sa` login, then exits.
3. **dbsetup** – creates the limited `catalog_app` login and grants it only what the API needs, then exits.
4. **api** – starts only after both one-shot steps exited successfully, and connects as `catalog_app`.

From a fresh clone and a filled-in `.env`, this produces a migrated, running API with no manual steps. The first build can take several minutes on a slow connection (NuGet packages are downloaded inside Docker).

- API: `http://localhost:8080`
- Swagger (Development only): `http://localhost:8080/swagger`
- Health check: `http://localhost:8080/health` (returns `Healthy`, or `503 Unhealthy` when the database is down)
- SQL Server is also published on `localhost:1433` so you can inspect it with SSMS or the VS Code MSSQL extension. Remove that port mapping in a real deployment.

To wipe the database and start over: `docker compose down -v` (this deletes all data).

## Run locally with `dotnet run`

The app does **not** migrate the database itself, so run the migrations first.

```bash
docker compose up -d db
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ProductCatalogDb;User Id=sa;Password=<DB_PASSWORD>;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "<at least 32 characters>"
dotnet user-secrets set "Seed:AdminUsername" "admin"
dotnet user-secrets set "Seed:AdminPassword" "<your admin password>"
dotnet ef database update
dotnet run
```

After pulling a change that adds a migration, run `dotnet ef database update` again.

## Run the tests

```bash
dotnet test ProductCatalogApi.Tests
```

Docker must be running: integration tests start a real SQL Server container through Testcontainers, apply the real migrations, and delete it afterwards. Run the command against the test project (not from the repo root without a project): the repo has no solution file, so a bare `dotnet test` in the root would only build the API project and run no tests.

- **Unit tests** use the EF Core InMemory provider (fast, logic only).
- **Integration tests** use real SQL Server, because InMemory has no locks, no stored procedures, no constraint enforcement, and compares strings case-sensitively.

Required integration tests include: stock adjustment success (stock updated, one `StockMovements` row), adjustment that would go negative (409, stock unchanged, no row), missing product (404), case-insensitive usernames, and a concurrency test of 20 parallel requests.

## Stock adjustments

`POST /api/products/{id}/stock-adjustments` (any logged-in user)

```json
{ "delta": -3, "reason": "Sold in store" }
```

| Result | Status |
|---|---|
| Success, returns `{ "productId": ..., "newStock": ... }` | 200 |
| Invalid body (zero delta, delta outside ±1000, missing or too long reason) | 400 |
| Product not found (procedure error 50002) | 404 |
| Stock would go below zero (procedure error 50001) | 409 |
| No / invalid token | 401 |

The service calls `dbo.usp_AdjustStock`, which locks the product row with `UPDLOCK, ROWLOCK`, rejects negative stock, updates the stock and writes an audit row to `StockMovements` (`Delta`, `StockAfter`, `Reason`, `CreatedBy` from the JWT, `CreatedAt` in UTC). Deleting a product also deletes its movement rows (cascade).

## Authentication and Authorization

The API uses JWT bearer tokens.

- Public: `/api/auth/register`, `/api/auth/login`, `/api/auth/refresh`, `/health`
- Protected: create/update/delete on products and categories, stock adjustments, reports
- Admin only: deleting products and categories, updating a user's role

Usernames are unique and case-insensitive (`admin` and `Admin` cannot both register). This relies on the database collation being case-insensitive, and an integration test checks it.

## API Endpoints

### Auth

| Method | Route | Description |
|---|---|---|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and receive access + refresh tokens |
| POST | `/api/auth/refresh` | Refresh JWT using a refresh token |
| POST | `/api/auth/logout` | Revoke the supplied refresh token |
| PUT | `/api/auth/user` | Update a user role (admin only) |

### Products

| Method | Route | Description |
|---|---|---|
| GET | `/api/products` | Get paginated products |
| GET | `/api/products/{id}` | Get product by ID |
| POST | `/api/products` | Create product |
| PUT | `/api/products/{id}` | Update product |
| DELETE | `/api/products/{id}` | Delete product |
| POST | `/api/products/{id}/stock-adjustments` | Adjust stock (see above) |

Query parameters for the product list: `pageNumber`, `pageSize`, `search`, `categoryId`, `minPrice`.

### Categories

| Method | Route | Description |
|---|---|---|
| GET | `/api/categories` | Get all categories |
| GET | `/api/categories/{id}` | Get category by ID |
| GET | `/api/categories/{id}/products` | Get products in a category |
| POST | `/api/categories` | Create category |
| PUT | `/api/categories/{id}` | Update category |
| DELETE | `/api/categories/{id}` | Delete category |

### Reports

| Method | Route | Description |
|---|---|---|
| GET | `/api/reports/inventory-value` | Total inventory value |
| GET | `/api/reports/most-expensive` | Most expensive product |
| GET | `/api/reports/out-of-stock` | All out-of-stock products |
| GET | `/api/reports/category-stats` | Statistics per category |

### Health

| Method | Route | Description |
|---|---|---|
| GET | `/health` | Database connectivity check |

The included `ProductCatalogApi.http` file covers these endpoints, including the 400, 404 and 409 stock cases (VS Code REST Client).

## Design decisions

**Why a stored procedure for stock?** The check ("is there enough stock?") and the change must happen in one locked step. If C# read the stock and then wrote it, two requests could both read the same value and both pass the check (a lost update). Keeping the lock, the check, the update and the audit row together in the database makes that impossible. The cost: logic lives in two places, and the procedure is harder to unit-test (hence the real-database integration tests).

**Who owns the transaction?** The procedure joins a caller's transaction if one is open, and otherwise starts and commits its own. It is safe by default (the `UPDLOCK` only protects while a transaction is open), and still composable: if .NET later needs "adjust stock and write an outbox row" atomically, it opens the transaction and the procedure joins it. Cost: when joined, the row lock lasts until the outer code commits, so outer transactions must stay short.

**Why migrations are not run at app startup.** With several replicas, every instance would try to change the same database at once. EF Core serialises this with a database lock (visible as `__EFMigrationsLock` in the migrator log), but it is still the wrong place: the API's login would need schema-changing rights, a slow or failing migration would block every instance, and old instances would serve traffic while the schema changes. Migrations are a one-shot deployment step (the `migrator` service) using an admin connection.

**Why the API does not connect as `sa`.** If the API were ever compromised (for example through SQL injection), the attacker would inherit the login's rights. `sa` can do anything on the server. `catalog_app` can only read and write this database's tables and execute `usp_AdjustStock`; it cannot create or drop tables.

**Cascade delete.** Deleting a product deletes its stock movements. The trade-off: the audit history only lives as long as the product.

## Concurrency experiments (usp_AdjustStock)

Test: a product with stock 10, 20 parallel requests of delta -1 (released together with a `TaskCompletionSource` gate).
Expected: exactly 10 x 200, 10 x 409, final stock 0, 10 `StockMovements` rows.

### With `UPDLOCK, ROWLOCK` (the final version)
Passed 5 out of 5 runs: 10 x 200, 10 x 409, final stock 0, 10 movement rows.
Why: the first request locks the row; the others wait at the SELECT. Each one then reads the already-updated stock, so the 11th request sees 0 and is refused.

### Without `UPDLOCK` (lost update)
Result: `200 x19, 409 x1`, final stock 0, 19 movement rows, sumDelta = -19, but only 10 distinct `StockAfter` values. Another run gave `200 x16, 409 x4`.
Why: a plain SELECT does not hold a lock that keeps other requests from reading the same stock: under READ COMMITTED it releases its shared lock immediately, and under READ_COMMITTED_SNAPSHOT it takes none. So many requests read the same stock, pass the check, and write back stale results. 19 sales were accepted for 10 items. The stock and the audit trail disagree, and nothing raised an error. The final stock is luck: it is whatever the last writer happened to read.

### With `HOLDLOCK` instead of `UPDLOCK` (deadlock)
Result: `200 x5, 500 x15`, final stock 5, 5 movement rows (the books balanced). The log confirmed the 500s: "Transaction ... was deadlocked ... chosen as the deadlock victim" (error 1205). This was one run.
Why: HOLDLOCK keeps a shared lock until the transaction ends. Several requests held shared locks on the same row, then all tried to upgrade to an exclusive lock for the UPDATE. Each waited for the others to release, so SQL Server killed one transaction per deadlock. UPDLOCK avoids this because update locks do not share with each other: the second request waits before it holds anything.

### Takeaway
UPDLOCK is correct. No hint loses data silently; HOLDLOCK fails loudly but keeps the data correct.

## Performance

See [`docs/performance.md`](docs/performance.md): execution plans and logical reads for the category filter, the name search and a report query, plus one justified index (`IX_Products_Price`, 1,670 to 6 logical reads for the most-expensive-product report).

## Notes

- `AdminSeeder` creates the first admin if the seed settings are present and no admin exists.
- All timestamps are stored as UTC `datetime2`; money is `decimal(18,2)`; strings have explicit lengths.
- `/health` reports unhealthy when the database is unreachable (try `docker compose stop db`).