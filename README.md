# Product Catalog Web API

A .NET Core Web API built for Task 02. It provides a RESTful backend service for managing products, categories, and database relationships using Entity Framework Core.

## Features
- **RESTful Endpoints**: Full CRUD operations for Categories and Products over HTTP.
- **Relational Data**: Genuine one-to-many relationship supporting category-specific product queries (`/api/categories/{id}/products`).
- **DTO Separation**: Request and response DTOs ensure internal database entities never cross the API boundary.
- **Robust Validation**: Data annotations (`[Required]`, `[MaxLength]`, `[Range]`) coupled with automatic `ModelState` validation.
- **Asynchronous Operations**: Fully async database calls (`ToListAsync`, `FindAsync`, `SaveChangesAsync`) to prevent thread blocking.
- **Audit Logging**: Structured logging (`ILogger`) tracking creates, updates, deletes, and exceptions.

---

## Prerequisites
- .NET 10 SDK (or compatible .NET runtime)
- EF Core CLI tools:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## How to Set the Connection String
1. Open `appsettings.json` in the root of the project.
2. Configure your connection string under the `ConnectionStrings` section:
   - **For Windows (SQL Server Express / LocalDB):**
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\mssqllocaldb;Database=ProductCatalogDb;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
     ```
   - **For macOS / Linux (SQLite):**
     Ensure your `Program.cs` uses `UseSqlite(...)` and adjust the connection string to a local file path:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=productcatalog.db"
     }
     ```

---

## How to Apply Migrations
Initialize the database from scratch by running the Entity Framework Core migration and update commands in the project directory:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## How to Run It
1. Clone the repository and clean out any residual artifacts (`bin/`, `obj/`, `.vs/`).
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Run the application:
   ```bash
   dotnet run
   ```
4. Test all endpoints and validation failure cases (such as `400 Bad Request` and `404 Not Found`) using the built-in `ProductCatalogApi.http` file or Postman.