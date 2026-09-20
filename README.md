# Product Catalog API

A .NET 10 Web API for managing a product catalog with categories, authentication, refresh tokens, and reporting features. The application exposes a REST API with SQL Server persistence, JWT authentication, and structured logging.

## Overview

This project is a backend service for a catalog system where users can:

- register and log in
- use JWT bearer tokens for protected endpoints
- refresh and revoke refresh tokens
- manage products and categories
- view inventory and category reports
- run an initial admin seeding step on startup

## Tech Stack

- ASP.NET Core Web API
- .NET 10
- Entity Framework Core
- SQL Server
- JWT authentication
- Argon2 password hashing
- Serilog for logging
- Swagger / OpenAPI
- xUnit for automated tests

## Project Structure

- `Program.cs` – app startup, dependency injection, authentication, Swagger, database migration, and admin seeding
- `Controllers/` – API endpoints for auth, products, categories, and reports
- `Services/` – business logic and token helpers
- `Data/` – EF Core context
- `Models/` – entity classes
- `DTOs/` – request and response models
- `Interfaces/` – service contracts
- `Extensions/` – helper extensions
- `Migrations/` – EF Core generated migration files
- `ProductCatalogApi.Tests/` – test project

## Prerequisites

- .NET 10 SDK
- SQL Server instance or local SQL Server Express / LocalDB
- Optional: Docker Desktop for running the app with Docker Compose
- EF Core CLI tools if you want to manage migrations manually

Install the EF tool if needed:

```bash
dotnet tool install --global dotnet-ef
```

## Configuration

The API requires a SQL Server connection string and JWT settings. These are configured in `appsettings.json` and can also be overridden through environment variables or user secrets.

Example `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ProductCatalogDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Jwt": {
    "Key": "YourSuperSecureKeyAtLeast32Chars!",
    "Issuer": "ProductCatalogApi",
    "Audience": "ProductCatalogApi",
    "ExpirationMinutes": 15
  },
  "PasswordHashing": {
    "TimeCost": 3,
    "MemoryCost": 65536,
    "Lanes": 4,
    "HashLength": 32
  },
  "Seed": {
    "AdminUsername": "admin",
    "AdminPassword": "AdminStrongPassword123!"
  },
  "AllowedHosts": "*"
}
```

Important:

- `Jwt:Key` must be at least 32 characters long.
- `Seed:AdminUsername` and `Seed:AdminPassword` are used by `AdminSeeder` to create the initial admin account at startup.

## Database Setup

The app automatically applies migrations on startup via `Database.Migrate()` in `Program.cs`.

If you want to manage migrations manually, run:

```bash
dotnet ef database update
```

To create new migrations:

```bash
dotnet ef migrations add <MigrationName>
```

## Running the Application

From the project root:

```bash
dotnet restore
dotnet build
dotnet run
```

The app listens on the default ASP.NET Core local port and exposes Swagger at:

```text
https://localhost:<port>/swagger
```

## Docker Setup

A Docker Compose file is included for SQL Server and the API.

To start the stack:

```bash
docker compose up --build
```

This uses environment variables:

- `DB_PASSWORD`
- `JWT_KEY`
- `ADMIN_SEED_PASSWORD`

The API is exposed on port `8080` and SQL Server on port `1433`.

## Authentication and Authorization

The API uses JWT bearer tokens.

### Public routes
- `/api/auth/register`
- `/api/auth/login`
- `/api/auth/refresh`

### Protected routes
- `/api/products` create/update/delete actions
- `/api/categories` create/update/delete actions
- `/api/reports` inventory and stats endpoints

Role-based access:

- Admin role can delete products and categories
- `UserUpdate` endpoint is restricted to admin-only access

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

Query parameters for product listing:

- `pageNumber`
- `pageSize`
- `search`
- `categoryId`
- `minPrice`

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

## Testing

The solution contains automated tests in the `ProductCatalogApi.Tests` project.

Run the tests with:

```bash
dotnet test
```

The test suite covers:

- authentication flows
- product logic
- category logic
- integration behavior through the test host
- refresh-token rotation and reuse detection

## Notes

- The project uses EF Core migrations and runs them automatically on startup.
- `AdminSeeder` ensures that a default admin is created if configuration is present and no admin exists.
- The included `ProductCatalogApi.http` file can be used to test endpoints directly in Visual Studio Code or VS.

## License

This project is intended for learning and internal task use unless otherwise specified by the owner.
