using Microsoft.EntityFrameworkCore;

namespace ProductCatalogApi;

public class ReportsService : IReportsService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReportsService> _logger;

    public ReportsService(AppDbContext context, ILogger<ReportsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ReportsDto>> GetInventoryValueAsync()
    {
        var total = await _context.Products
            .SumAsync(p => p.Price * p.Stock);

        return Result<ReportsDto>.Success(new ReportsDto
        {
            TotalValue = Math.Round(total, 2)
        });
    }

    public async Task<Result<ProductDto?>> GetMostExpensiveProductAsync()
    {
        var product = await _context.Products
            .OrderByDescending(p => p.Price)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            _logger.LogWarning("Attempted to retrieve the most expensive product when no products exist");
            return Result<ProductDto?>.Failure(ErrorType.NotFound, "No products exist");
        }

        return Result<ProductDto?>.Success(product.ToDto());
    }

    public async Task<Result<IEnumerable<OutOfStockProductDto>>> GetOutOfStockProductsAsync()
    {
        var products = await _context.Products
            .Where(p => p.Stock == 0)
            .Select(p => new OutOfStockProductDto
            {
                Id = p.Id,
                Name = p.Name,
                CategoryId = p.CategoryId
            })
            .ToListAsync();

        return Result<IEnumerable<OutOfStockProductDto>>.Success(products);
    }

    public async Task<Result<IEnumerable<CategoryStatsDto>>> GetCategoryStatsAsync()
    {
        var stats = await _context.Products
            .GroupBy(p => new { p.CategoryId, p.Category!.Name })
            .Select(g => new CategoryStatsDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                ProductCount = g.Count(),
                AveragePrice = g.Average(p => p.Price)
            })
            .ToListAsync();

        var result = stats.Select(stat => new CategoryStatsDto
        {
            CategoryId = stat.CategoryId,
            CategoryName = stat.CategoryName,
            ProductCount = stat.ProductCount,
            AveragePrice = Math.Round(stat.AveragePrice, 2)
        });

        return Result<IEnumerable<CategoryStatsDto>>.Success(result);
    }
}
