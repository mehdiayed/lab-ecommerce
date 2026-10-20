using ECommerce.Catalog.Api.Data;
using ECommerce.Catalog.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ECommerce.Catalog.Api.Endpoints;

public static class CatalogEndpoints
{
    private const int SkuMaxLength = 40;

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder routes)
    {
        var categories = routes.MapGroup("/api/categories").WithTags("Catalog");

        categories.MapGet("/", async (CatalogDbContext db) =>
            await db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync())
            .WithName("GetCategories");

        var group = routes.MapGroup("/api/products").WithTags("Catalog");

        group.MapGet("/", async (int? categoryId, string? search, CatalogDbContext db) =>
        {
            var query = db.Products.AsNoTracking();

            if (categoryId is not null)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{EscapeLike(search.Trim())}%";
                query = query.Where(p =>
                    EF.Functions.ILike(p.Name, pattern, "\\") ||
                    EF.Functions.ILike(p.Sku, pattern, "\\") ||
                    (p.Description != null && EF.Functions.ILike(p.Description, pattern, "\\")));
            }

            return await query.OrderBy(p => p.Id).ToListAsync();
        })
        .WithName("GetProducts");

        group.MapGet("/{id:int}", async (int id, CatalogDbContext db) =>
            await db.Products.FindAsync(id) is { } product
                ? Results.Ok(product)
                : Results.NotFound())
            .WithName("GetProductById");

        group.MapPost("/", async (CreateProductRequest request, CatalogDbContext db) =>
        {
            var errors = Validate(request);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            var sku = request.Sku!.Trim();

            if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.CategoryId)] = [$"Category {request.CategoryId} does not exist."]
                });
            }

            if (await db.Products.AnyAsync(p => p.Sku == sku))
            {
                return Results.Conflict(new { error = $"A product with SKU '{sku}' already exists." });
            }

            var product = new Product
            {
                Sku = sku,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                AvailableStock = request.AvailableStock,
                CategoryId = request.CategoryId!.Value
            };

            db.Products.Add(product);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // Lost a race with a concurrent create using the same SKU.
                return Results.Conflict(new { error = $"A product with SKU '{sku}' already exists." });
            }

            return Results.CreatedAtRoute("GetProductById", new { id = product.Id }, product);
        })
        .WithName("CreateProduct");

        return routes;
    }

    private static Dictionary<string, string[]> Validate(CreateProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Sku))
        {
            errors[nameof(request.Sku)] = ["Sku is required."];
        }
        else if (request.Sku.Trim().Length > SkuMaxLength)
        {
            errors[nameof(request.Sku)] = [$"Sku must be at most {SkuMaxLength} characters."];
        }

        if (request.CategoryId is null or <= 0)
        {
            errors[nameof(request.CategoryId)] = ["CategoryId is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors[nameof(request.Name)] = ["Name is required."];
        }

        if (request.Price < 0)
        {
            errors[nameof(request.Price)] = ["Price must be greater than or equal to 0."];
        }

        if (request.AvailableStock < 0)
        {
            errors[nameof(request.AvailableStock)] = ["AvailableStock must be greater than or equal to 0."];
        }

        return errors;
    }

    // Escape LIKE wildcards so user input is matched literally (escape char: backslash).
    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int AvailableStock,
    string? Sku,
    int? CategoryId);
