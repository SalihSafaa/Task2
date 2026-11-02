using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ProductCatalogApi.Tests;

public class StockAdjustmentTests : IClassFixture<SqlServerWebApplicationFactory>
{
    private readonly SqlServerWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public StockAdjustmentTests(SqlServerWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AdjustStock_Valid_UpdatesStockAndWritesOneMovement()
    {
        await AuthenticateAsync();
        var productId = await SeedProductAsync(stock: 10);

        var response = await _client.PostAsJsonAsync(
            $"/api/products/{productId}/stock-adjustments",
            new { delta = -3, reason = "Sold in store" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<StockAdjustmentResultDto>();
        Assert.Equal(7, body!.NewStock);
        Assert.Equal(7, await GetStockAsync(productId));
        Assert.Equal(1, await CountMovementsAsync(productId));
    }
    [Fact]
    public async Task AdjustStock_WouldGoNegative_Returns409AndChangesNothing()
    {
        await AuthenticateAsync();
        var productId = await SeedProductAsync(stock: 2);

        var response = await _client.PostAsJsonAsync(
            $"/api/products/{productId}/stock-adjustments",
            new { delta = -3, reason = "Too many" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(2, await GetStockAsync(productId));
        Assert.Equal(0, await CountMovementsAsync(productId));
    }

    [Fact]
    public async Task AdjustStock_MissingProduct_Returns404()
    {
        await AuthenticateAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/products/99999999/stock-adjustments",
            new { delta = -1, reason = "No such product" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task Register_SameUsernameDifferentCase_Returns409()
    {
        var username = $"case_{Guid.NewGuid():N}";

        var first = await _client.PostAsJsonAsync("/api/auth/register",
            new { username, password = "TestPassword123!" });
        first.EnsureSuccessStatusCode();

        var second = await _client.PostAsJsonAsync("/api/auth/register",
            new { username = username.ToUpperInvariant(), password = "TestPassword123!" });

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }
    [Fact]
    public async Task AdjustStock_20ParallelRequests_ExactlyTenSucceed()
    {
        await AuthenticateAsync();
        var productId = await SeedProductAsync(stock: 10);

        // The gate makes all 20 requests start at the same moment,
        // instead of one-by-one as each task is created.
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(async () =>
        {
            await gate.Task;
            var response = await _client.PostAsJsonAsync(
                $"/api/products/{productId}/stock-adjustments",
                new { delta = -1, reason = "parallel" });
            return response.StatusCode;
        })).ToList();

        gate.SetResult();
        var statuses = await Task.WhenAll(tasks);

        var summary = string.Join(", ",
            statuses.GroupBy(s => s).Select(g => $"{(int)g.Key} x{g.Count()}"));

        Assert.True(statuses.Count(s => s == HttpStatusCode.OK) == 10, $"Statuses: {summary}");
        Assert.True(statuses.Count(s => s == HttpStatusCode.Conflict) == 10, $"Statuses: {summary}");
        Assert.Equal(0, await GetStockAsync(productId));
        Assert.Equal(10, await CountMovementsAsync(productId));
    }
    // ---------- helpers ----------

    private async Task AuthenticateAsync()
    {
        var username = $"user_{Guid.NewGuid():N}";
        var register = await _client.PostAsJsonAsync("/api/auth/register",
            new { username, password = "TestPassword123!" });
        register.EnsureSuccessStatusCode();

        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new { username, password = "TestPassword123!" });
        login.EnsureSuccessStatusCode();

        var body = await login.Content.ReadFromJsonAsync<LoginResponseDto>();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", body!.AccessToken);
    }

    private async Task<int> SeedProductAsync(int stock)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var category = new Category { Name = $"cat_{Guid.NewGuid():N}" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = new Product { Name = "Test Product", Price = 9.99m, Stock = stock, CategoryId = category.Id };
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product.Id;
    }

    private async Task<int> GetStockAsync(int productId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Products.AsNoTracking()
            .Where(p => p.Id == productId).Select(p => p.Stock).SingleAsync();
    }

    private async Task<int> CountMovementsAsync(int productId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.StockMovements.CountAsync(m => m.ProductId == productId);
    }
}