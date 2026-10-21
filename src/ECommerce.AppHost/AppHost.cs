var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL for Catalog — fixed host port, data kept in a named Docker volume.
// The password is a fixed secret parameter: with a persistent volume, a regenerated
// password would no longer match the one the database was initialised with.
var pgPassword = builder.AddParameter("pg-password", secret: true);
var postgres = builder.AddPostgres("postgres", password: pgPassword)
    .WithHostPort(5433)
    .WithDataVolume("ecommerce-pgdata");
var catalogDb = postgres.AddDatabase("catalogdb");

// Catalog microservice.
var catalog = builder.AddProject<Projects.ECommerce_Catalog_Api>("catalog")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);

// Ordering microservice — depends on Catalog (resolved via service discovery).
var ordering = builder.AddProject<Projects.ECommerce_Ordering_Api>("ordering")
    .WithReference(catalog)
    .WaitFor(catalog);

// API Gateway (YARP) — the single entry point for the services.
var gateway = builder.AddProject<Projects.ECommerce_Gateway>("gateway")
    .WithReference(catalog)
    .WithReference(ordering)
    .WaitFor(catalog)
    .WaitFor(ordering);

// Blazor web frontend — talks to the services through the gateway.
builder.AddProject<Projects.ECommerce_Web>("web")
    .WithReference(gateway)
    .WaitFor(gateway)
    .WithExternalHttpEndpoints();

builder.Build().Run();
