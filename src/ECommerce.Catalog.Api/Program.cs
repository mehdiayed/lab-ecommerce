using ECommerce.Catalog.Api.Data;
using ECommerce.Catalog.Api.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Aspire: telemetry, health checks, resilience, service discovery.
builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.AddNpgsqlDbContext<CatalogDbContext>("catalogdb");

var app = builder.Build();

// Aspire default endpoints (/health, /alive).
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapCatalogEndpoints();

// Apply pending migrations (schema + seed) on startup, local development only.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

// Makes the Program class visible to the integration tests (WebApplicationFactory<Program>).
public partial class Program { }
