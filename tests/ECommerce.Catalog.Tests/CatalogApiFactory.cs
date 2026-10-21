// Fabrique de test : démarre l'API Catalog en mémoire, branchée sur un VRAI PostgreSQL
// jetable lancé par Testcontainers (un conteneur Docker par exécution de la suite).
//
// Pourquoi pas un provider in-memory ? Parce qu'il ne connaît ni les contraintes CHECK,
// ni l'index unique sur le SKU, ni les séquences d'identité : les tests passeraient
// pendant que la production échouerait. On teste contre le moteur qu'on déploie.
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Xunit;

namespace ECommerce.Catalog.Tests;

public sealed class CatalogApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Image épinglée : un test reproductible ne dépend pas de "latest".
    // Identifiants de conteneur jetable, local, détruit en fin de suite : pas un secret.
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("catalogdb")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();

    // La chaîne de connexion du conteneur remplace celle qu'Aspire injecterait.
    // Au démarrage (environnement Development), Program.cs applique les migrations.
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:catalogdb", _postgres.GetConnectionString());

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
