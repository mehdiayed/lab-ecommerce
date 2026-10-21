# ECommerce.Catalog.Api

**Type:** ASP.NET Core minimal API
**Depends on:** ECommerce.ServiceDefaults, EF Core + Npgsql (PostgreSQL via Aspire)

The Catalog microservice. Owns products. Completely standalone — it knows nothing about
Ordering or the Gateway.

## Files

| File | Responsibility |
|---|---|
| `Models/Product.cs` | The `Product` entity: Id, Sku, Name, Description, Price, AvailableStock, CategoryId. |
| `Models/Category.cs` | The `Category` entity: Id, Name. |
| `Migrations/` | EF Core migrations (`InitialCreate`). Applied at startup in Development. |
| `Data/CatalogDbContext.cs` | EF Core context. Configures the schema (unique SKU/category name, FK `RESTRICT`, CHECK constraints) and **seeds 3 categories and 4 demo products**. |
| `Endpoints/CatalogEndpoints.cs` | REST routes grouped under `/api/products` + the `CreateProductRequest` DTO. |
| `Program.cs` | Composition root: `AddServiceDefaults`, `AddNpgsqlDbContext`, OpenAPI, map endpoints, apply migrations (Development). |

## Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/api/categories` | List categories |
| GET | `/api/products` | List products; optional `?categoryId=` and `?search=` (name, SKU, description) |
| GET | `/api/products/{id}` | Get one product (404 if missing) |
| POST | `/api/products` | Create a product (requires `sku`, `categoryId`; 400 unknown category / invalid input, 409 duplicate SKU) |

## Patterns

- **Minimal API** with `MapGroup("/api/products")` and named endpoints.
- **DTO separation** — `CreateProductRequest` is the API contract, kept distinct from the
  `Product` entity.
- `AsNoTracking()` on read queries for efficiency.

## Storage

PostgreSQL, declared in the AppHost (`AddPostgres("postgres")`, database `catalogdb`, host port 5433,
named volume `ecommerce-pgdata`). Data survives restarts. The schema is versioned with EF Core
migrations (`dotnet ef migrations add <Name> --project src/ECommerce.Catalog.Api`).
