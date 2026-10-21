// Tests d'intégration de la création de commande (Ordering.Api), avec un faux Catalog.
//
// Le test du total fige un comportement MÉTIER : une commande de 2 moniteurs coûte
// 2 x 329,00. C'est exactement ce qu'un test doit protéger — pas un détail d'implémentation.
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ECommerce.Ordering.Tests;

public class OrderingEndpointsTests(OrderingApiFactory factory) : IClassFixture<OrderingApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private sealed record OrderResult(int Id, string Status, decimal Total);

    [Fact]
    public async Task CreateOrder_QuantitesMultiples_TotalEgalPrixFoisQuantite()
    {
        var commande = new { Customer = "Client Test", Items = new[] { new { ProductId = 3, Quantity = 2 }, new { ProductId = 1, Quantity = 1 } } };

        var response = await _client.PostAsJsonAsync("/api/orders", commande);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderResult>();
        Assert.Equal("Confirmed", order!.Status);
        Assert.Equal(2 * 329.00m + 119.99m, order.Total);
    }

    [Fact]
    public async Task CreateOrder_SansLigne_Retourne400()
    {
        var response = await _client.PostAsJsonAsync("/api/orders", new { Customer = "Client Test", Items = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_QuantiteNulleOuNegative_Retourne400()
    {
        var commande = new { Customer = "Client Test", Items = new[] { new { ProductId = 1, Quantity = 0 } } };

        var response = await _client.PostAsJsonAsync("/api/orders", commande);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_ProduitInconnu_Retourne400()
    {
        var commande = new { Customer = "Client Test", Items = new[] { new { ProductId = 999, Quantity = 1 } } };

        var response = await _client.PostAsJsonAsync("/api/orders", commande);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
