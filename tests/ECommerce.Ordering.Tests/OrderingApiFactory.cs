// Fabrique de test d'Ordering.Api : l'application réelle, avec un FAUX service Catalog.
//
// Ordering appelle Catalog en HTTP pour valider chaque produit. En test, on remplace le
// handler HTTP du client typé par un catalogue figé : les tests restent rapides,
// déterministes, et ne dépendent d'aucun autre service.
using System.Net;
using System.Net.Http.Json;
using ECommerce.Ordering.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Ordering.Tests;

public sealed class OrderingApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Le service discovery doit pouvoir résoudre "catalog" ; l'adresse n'est jamais
        // appelée pour de vrai, le handler ci-dessous intercepte tout.
        builder.UseSetting("services:catalog:http:0", "http://catalog.test");

        builder.ConfigureTestServices(services =>
            services.AddHttpClient<CatalogServiceClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => new FakeCatalogHandler()));
    }
}

/// <summary>Catalogue figé : deux produits connus, tout le reste est 404.</summary>
internal sealed class FakeCatalogHandler : HttpMessageHandler
{
    private static readonly Dictionary<int, CatalogProduct> Products = new()
    {
        [1] = new CatalogProduct(1, "Mechanical Keyboard", "Hot-swappable RGB keyboard", 119.99m, 50),
        [3] = new CatalogProduct(3, "4K Monitor", "27-inch IPS display", 329.00m, 25),
    };

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var segment = request.RequestUri!.Segments[^1];
        var response = int.TryParse(segment, out var id) && Products.TryGetValue(id, out var product)
            ? new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(product) }
            : new HttpResponseMessage(HttpStatusCode.NotFound);

        return Task.FromResult(response);
    }
}
