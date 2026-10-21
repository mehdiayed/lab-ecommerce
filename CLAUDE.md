# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

.NET 10 / ASP.NET Core e-commerce app split into microservices, orchestrated by .NET Aspire. It is the running example of a Claude Code training course (README.md is in French). It is intentionally minimal: no tests, no Dockerfiles, no CI, and a single real database: PostgreSQL for Catalog (Ordering is still EF Core in-memory). Docker is required for Catalog's PostgreSQL container; one-time setup: `dotnet user-secrets set "Parameters:pg-password" "<password>" --project src/ECommerce.AppHost`.

## Commands

Requires the .NET 10 SDK and, once, a trusted dev cert (`dotnet dev-certs https --trust`), otherwise the Aspire dashboard fails with `UntrustedRoot`.

```bash
dotnet build ECommerce.slnx
dotnet run --project src/ECommerce.AppHost   # starts every service + Aspire dashboard
```

- Run from the repo root. Use the `Login URL` printed in the terminal for the dashboard; ports change on every launch. `Ctrl+C` stops everything.
- There is no test project, so no test or lint command exists yet.
- Individual services can be run on their own, but cross-service URLs only resolve when started via the AppHost.

## Architecture

Web (Blazor) → Gateway (YARP) → Catalog.Api / Ordering.Api. Ordering calls Catalog synchronously over HTTP to validate each product on order creation.

- **AppHost** ([AppHost.cs](src/ECommerce.AppHost/AppHost.cs)) is the single source of truth for service names, `WithReference` dependencies and `WaitFor` startup order. Services never hard-code each other's addresses. They use Aspire service discovery with names like `https+http://catalog`. When adding a service or dependency, register it here.
- **ServiceDefaults** ([Extensions.cs](src/ECommerce.ServiceDefaults/Extensions.cs)) is shared by every service: OpenTelemetry, health checks, HTTP resilience, service discovery.
- **Gateway** routes `/catalog/*` and `/ordering/*` via YARP rules in `src/ECommerce.Gateway/appsettings.json`. The Web app talks only to the gateway (clients in `ECommerce.Web/Services/`).
- **Catalog.Api / Ordering.Api** are minimal-API services (`Endpoints/`, `Models/`, `Data/` with an EF Core DbContext and demo seed data; Catalog uses PostgreSQL with migrations in `Migrations/`, Ordering is in-memory). Ordering's [CatalogServiceClient.cs](src/ECommerce.Ordering.Api/Services/CatalogServiceClient.cs) performs the validation call to Catalog.
- Gateway routes strip their prefix (`PathRemovePrefix`), so services themselves expose `/api/products` and `/api/orders`, not `/catalog/...`. A new service needs a route, a cluster and an AppHost `WithReference`.
- Order creation snapshots `ProductName` and `UnitPrice` from Catalog into `OrderItem`. It only checks that the product exists. `AvailableStock` is fetched but never validated or decremented.
- Ordering serializes enums (e.g. `OrderStatus`) as strings via `ConfigureHttpJsonOptions` in its `Program.cs`.
- Catalog data is persisted in PostgreSQL (Aspire resource `postgres`, database `catalogdb`, host port 5433, Docker volume `ecommerce-pgdata`); migrations are applied at startup in Development. Ordering data is not persisted: restarting resets it.
- Catalog schema: `Categories` (unique name), `Products` (unique `Sku` ≤ 40 chars, `CategoryId` FK with `ON DELETE RESTRICT`, CHECK price ≥ 0 and stock ≥ 0). `POST /api/products` requires `Sku` and `CategoryId` (400 if the category is unknown, 409 if the SKU exists); `GET /api/products` accepts `?categoryId=` and `?search=`; `GET /api/categories` lists categories.
- REST routes through the gateway: `/catalog/api/products[/{id}]`, `/ordering/api/orders[/{id}]`. Services expose OpenAPI at `/openapi/v1.json` in Development.
- Each project has its own small README.md describing its files.

## Git conventions

- Branches: `feat/`, `fix/`, `chore/`, `test/`, `ci/` prefixes. Never work directly on `main`.
- Commits: Conventional Commits, in English, imperative mood. Allowed scopes: `catalog`, `ordering`, `gateway`, `web`, `db`, `ci` (e.g. `feat(ordering): validate stock on order creation`).
