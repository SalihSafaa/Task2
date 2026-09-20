using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ProductCatalogApi.Tests;

public class ProductsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProductsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_Anonymous_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/products");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithoutToken_ReturnsUnauthorized()
    {
        var payload = new
        {
            name = "Unauthorized Laptop",
            price = 999.99m,
            stock = 5,
            categoryId = 1
        };

        var response = await _client.PostAsJsonAsync("/api/products", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FullFlow_RegisterLoginThenCreateProduct_Succeeds()
    {
        var username = $"user_{Guid.NewGuid():N}";
        await RegisterAsync(username, "TestPassword123!");
        var accessToken = await LoginAsync(username, "TestPassword123!");

        var categoryId = await SeedCategoryAsync("Electronics");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var payload = new
        {
            name = "Integration-Tested Laptop",
            price = 1499.99m,
            stock = 10,
            categoryId
        };

        var response = await _client.PostAsJsonAsync("/api/products", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(created);
        Assert.Equal("Integration-Tested Laptop", created!.Name);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidCategory_ReturnsBadRequest()
    {
        var username = $"user_{Guid.NewGuid():N}";
        await RegisterAsync(username, "TestPassword123!");
        var accessToken = await LoginAsync(username, "TestPassword123!");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var payload = new { name = "Ghost Product", price = 10m, stock = 1, categoryId = 999999 };

        var response = await _client.PostAsJsonAsync("/api/products", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Category with ID 999999 was not found. Please select a valid category.", problem!.Title);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
    }

    [Fact]
    public async Task DeleteProduct_AsRegularUser_ReturnsForbidden()
    {
        var username = $"user_{Guid.NewGuid():N}";
        await RegisterAsync(username, "TestPassword123!"); // registers with Role = "User"
        var accessToken = await LoginAsync(username, "TestPassword123!");

        var categoryId = await SeedCategoryAsync("Books");
        var productId = await SeedProductAsync("Some Book", categoryId);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.DeleteAsync($"/api/products/{productId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_AsAdmin_Succeeds()
    {
        var username = $"admin_{Guid.NewGuid():N}";
        await RegisterAsync(username, "TestPassword123!");
        await PromoteToAdminAsync(username);
        var accessToken = await LoginAsync(username, "TestPassword123!");

        var categoryId = await SeedCategoryAsync("Toys");
        var productId = await SeedProductAsync("Toy Robot", categoryId);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.DeleteAsync($"/api/products/{productId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task RegisterAsync(string username, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { username, password });
        response.EnsureSuccessStatusCode();
    }

    private async Task<string> LoginAsync(string username, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username, password });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(body);
        return body!.AccessToken;
    }

    private async Task PromoteToAdminAsync(string username)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await context.Users.SingleAsync(u => u.Username == username);
        user.Role = "Admin";
        await context.SaveChangesAsync();
    }

    private async Task<int> SeedCategoryAsync(string name)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var category = new Category { Name = name };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category.Id;
    }

    private async Task<int> SeedProductAsync(string name, int categoryId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var product = new Product { Name = name, Price = 9.99m, Stock = 1, CategoryId = categoryId };
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product.Id;
    }
}