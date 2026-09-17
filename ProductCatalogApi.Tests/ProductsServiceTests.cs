using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ProductCatalogApi.Tests;

public class ProductsServiceTests
{
    [Fact]
    public async Task CreateProductAsync_WithValidCategory_Succeeds()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics" });
        await context.SaveChangesAsync();

        var service = new ProductsService(context, NullLogger<ProductsService>.Instance);
        var dto = new CreateProductDto
        {
            Name = "Laptop",
            Price = 999.99m,
            Stock = 5,
            CategoryId = 1
        };

        var result = await service.CreateProductAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal("Laptop", result.Value!.Name);
        Assert.Equal(1, await context.Products.CountAsync());
    }
    [Fact]
    public async Task CreateProductAsync_WithInvalidCategory_Fails()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductsService(context, NullLogger<ProductsService>.Instance);
        var dto = new CreateProductDto
        {
            Name = "Laptop",
            Price = 999.99m,
            Stock = 5,
            CategoryId = 999
        };

        var result = await service.CreateProductAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Equal("Category with ID 999 was not found. Please select a valid category.", result.ErrorMessage);
    }
    [Fact]
    public async Task UpdateProductAsync_WithValidCategory_Succeeds()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductsService(context, NullLogger<ProductsService>.Instance);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics" });
        context.Products.Add(new Product { Id = 1, Name = "Old Laptop", Price = 899.99m, Stock = 3, CategoryId = 1 });
        await context.SaveChangesAsync();

        var UpdateDto = new UpdateProductDto
        {
            Name = "New Laptop",
            Price = 999.99m,
            Stock = 5,
            CategoryId = 1
        };

        var result = await service.UpdateProductAsync(1, UpdateDto);
        await context.SaveChangesAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal("New Laptop", context.Products.First().Name);
    }
    [Fact]
    public async Task UpdateProductAsync_WithInvalidCategory_Fails()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductsService(context, NullLogger<ProductsService>.Instance);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics" });
        context.Products.Add(new Product { Id = 1, Name = "Old Laptop", Price = 899.99m, Stock = 3, CategoryId = 1 });
        await context.SaveChangesAsync();

        var UpdateDto = new UpdateProductDto
        {
            Name = "New Laptop",
            Price = 999.99m,
            Stock = 5,
            CategoryId = 999
        };

        var result = await service.UpdateProductAsync(1, UpdateDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("Category with ID 999 was not found. Please select a valid category.", result.ErrorMessage);
    }
    [Fact]
    public async Task UpdateProductAsync_WithNonExistentProduct_Fails()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductsService(context, NullLogger<ProductsService>.Instance);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics" });
        await context.SaveChangesAsync();

        var UpdateDto = new UpdateProductDto
        {
            Name = "New Laptop",
            Price = 999.99m,
            Stock = 5,
            CategoryId = 1
        };

        var result = await service.UpdateProductAsync(999, UpdateDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("Product with ID 999 was not found.", result.ErrorMessage);
    }
    [Fact]
    public async Task DeleteProductAsync_WithExistingProduct_Succeeds()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductsService(context, NullLogger<ProductsService>.Instance);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics" });
        context.Products.Add(new Product { Id = 1, Name = "Laptop", Price = 999.99m, Stock = 5, CategoryId = 1 });
        await context.SaveChangesAsync();

        var result = await service.DeleteProductAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, await context.Products.CountAsync());
    }
    [Fact]
    public async Task DeleteProductAsync_WithNonExistentProduct_Fails()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductsService(context, NullLogger<ProductsService>.Instance);

        var result = await service.DeleteProductAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Equal("Product with ID 999 was not found.", result.ErrorMessage);
    }
    [Fact]
    public async Task GetProductAsync_WithExistingProduct_Succeeds()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductsService(context, NullLogger<ProductsService>.Instance);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics" });
        context.Products.Add(new Product { Id = 1, Name = "Laptop", Price = 999.99m, Stock = 5, CategoryId = 1 });
        await context.SaveChangesAsync();

        var result = await service.GetProductAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Laptop", result.Value!.Name);
    }
    [Fact]
    public async Task GetProductAsync_WithNonExistentProduct_Fails()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductsService(context, NullLogger<ProductsService>.Instance);

        var result = await service.GetProductAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Equal("Product with ID 999 was not found.", result.ErrorMessage);
    }
    [Fact]
    public async Task GetProductsAsync_ReturnsAllProducts()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductsService(context, NullLogger<ProductsService>.Instance);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics" });
        context.Products.Add(new Product { Id = 1, Name = "Laptop", Price = 999.99m, Stock = 5, CategoryId = 1 });
        context.Products.Add(new Product { Id = 2, Name = "Smartphone", Price = 499.99m, Stock = 10, CategoryId = 1 });
        await context.SaveChangesAsync();

        var query = new PaginationRequestDto();
        var result = await service.GetProductsAsync(query); //query needed

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.Products.Count());
    }

    [Fact]
    public async Task GetProductsAsync_Page2_ReturnsCorrectSlice()
    {
        // 5 products, page size 2 -> page 2 should be items 3 and 4
        // (ordered by Id), with TotalCount still reflecting all 5.
        // This is the test the old "ReturnsAllProducts" test didn't
        // actually cover: it only checked the raw row count, which
        // would still pass even if Skip/Take were broken.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductsService(context, NullLogger<ProductsService>.Instance);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics" });
        for (var i = 1; i <= 5; i++)
        {
            context.Products.Add(new Product
            {
                Id = i,
                Name = $"Product {i}",
                Price = 100m * i,
                Stock = i,
                CategoryId = 1
            });
        }
        await context.SaveChangesAsync();

        var query = new PaginationRequestDto { PageNumber = 2, PageSize = 2 };
        var result = await service.GetProductsAsync(query);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.Items.Count());
        Assert.Equal(new[] { "Product 3", "Product 4" }, result.Value.Items.Select(p => p.Name));
    }

    [Fact]
    public async Task GetProductsAsync_LastPage_ReturnsRemainderOnly()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductsService(context, NullLogger<ProductsService>.Instance);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics" });
        for (var i = 1; i <= 5; i++)
        {
            context.Products.Add(new Product
            {
                Id = i,
                Name = $"Product {i}",
                Price = 100m * i,
                Stock = i,
                CategoryId = 1
            });
        }
        await context.SaveChangesAsync();

        var query = new PaginationRequestDto { PageNumber = 3, PageSize = 2 };
        var result = await service.GetProductsAsync(query);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal("Product 5", result.Value.Items.Single().Name);
    }

    [Fact]
    public async Task GetProductsAsync_FilterByCategory_ReturnsOnlyMatchingProducts()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductsService(context, NullLogger<ProductsService>.Instance);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics" });
        context.Categories.Add(new Category { Id = 2, Name = "Books" });
        context.Products.Add(new Product { Id = 1, Name = "Laptop", Price = 999.99m, Stock = 5, CategoryId = 1 });
        context.Products.Add(new Product { Id = 2, Name = "Novel", Price = 12.99m, Stock = 20, CategoryId = 2 });
        await context.SaveChangesAsync();

        var query = new PaginationRequestDto { CategoryId = 2 };
        var result = await service.GetProductsAsync(query);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalCount);
        Assert.Equal("Novel", result.Value.Items.Single().Name);
    }
}