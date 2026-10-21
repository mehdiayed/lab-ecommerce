// Tests d'intégration des endpoints du catalogue, contre un vrai PostgreSQL (lab 04).
//
// Les cinq premiers tests viennent du lab 02 : ils n'ont PAS changé de sens. Seule la
// création de produit a dû évoluer, parce que le contrat a évolué (SKU + catégorie).
// C'est le rôle d'une suite de tests : signaler qu'un contrat a bougé, pas être réécrite
// pour passer.
using System.Net;
using System.Net.Http.Json;
using ECommerce.Catalog.Api.Models;
using Xunit;

namespace ECommerce.Catalog.Tests;

public class CatalogEndpointsTests(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // ---------------------------------------------------------------- hérités du lab 02

    [Fact]
    public async Task GetProducts_RetourneLeCatalogueSeede()
    {
        var products = await _client.GetFromJsonAsync<List<Product>>("/api/products");

        Assert.NotNull(products);
        Assert.Contains(products!, p => p.Sku == "KB-MX-001");
    }

    [Fact]
    public async Task GetProductById_IdInexistant_Retourne404()
    {
        var response = await _client.GetAsync("/api/products/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_IdNonEntier_NeProvoquePasDErreurServeur()
    {
        var response = await _client.GetAsync("/api/products/abc");

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_PuisLecture_RetourneLeProduitCree()
    {
        // Le premier INSERT après le seed : c'est lui qui révèle une séquence d'identité
        // non décalée (duplicate key -> 500). Voir HasIdentityOptions dans le DbContext.
        var requete = new { Sku = "KB-AZ-101", Name = "Clavier", Description = "AZERTY", Price = 49.90m, AvailableStock = 12, CategoryId = 1 };

        var creation = await _client.PostAsJsonAsync("/api/products", requete);

        Assert.Equal(HttpStatusCode.Created, creation.StatusCode);
        var relecture = await _client.GetFromJsonAsync<Product>(creation.Headers.Location);
        Assert.NotNull(relecture);
        Assert.Equal("KB-AZ-101", relecture!.Sku);
        Assert.Equal(49.90m, relecture.Price);
    }

    [Fact]
    public async Task Health_RepondOk_QuandLaBaseEstJoignable()
    {
        // Avec AddNpgsqlDbContext, /health inclut un contrôle de la base : un 200 prouve
        // que l'API joint réellement PostgreSQL. C'est ce que lira la readiness probe.
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ---------------------------------------------------------------- nouveaux (lab 04)

    [Fact]
    public async Task GetCategories_RetourneLesTroisCategoriesSeedees()
    {
        var categories = await _client.GetFromJsonAsync<List<Category>>("/api/categories");

        Assert.NotNull(categories);
        Assert.Equal(["Accessories", "Displays", "Peripherals"], categories!.Select(c => c.Name).Take(3));
    }

    [Fact]
    public async Task GetProducts_FiltreParCategorie_NeRetourneQueCetteCategorie()
    {
        var products = await _client.GetFromJsonAsync<List<Product>>("/api/products?categoryId=2");

        Assert.NotNull(products);
        Assert.NotEmpty(products!);
        Assert.All(products!, p => Assert.Equal(2, p.CategoryId));
    }

    [Fact]
    public async Task CreateProduct_SkuDejaExistant_Retourne409()
    {
        var doublon = new { Sku = "MS-WL-002", Name = "Copie", Description = (string?)null, Price = 10m, AvailableStock = 1, CategoryId = 1 };

        var response = await _client.PostAsJsonAsync("/api/products", doublon);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_CategorieInexistante_Retourne400()
    {
        var orphelin = new { Sku = "XX-NO-999", Name = "Orphelin", Description = (string?)null, Price = 10m, AvailableStock = 1, CategoryId = 4242 };

        var response = await _client.PostAsJsonAsync("/api/products", orphelin);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
