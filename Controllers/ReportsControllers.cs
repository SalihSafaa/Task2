namespace ProductCatalogApi;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(AppDbContext context, ILogger<ReportsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("inventory-value")]
    public async Task<ActionResult<ReportsDto>> GetInventoryValue()
    {
        var total = await _context.Products
            .SumAsync(p => p.Price * p.Stock);

        return Ok(new ReportsDto { TotalValue = total });
    }
    [HttpGet("most-expensive")]
    public async Task<ActionResult<ProductDto>> GetMostExpensiveProduct()
    {
        var product = await _context.Products
            .OrderByDescending(p => p.Price)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound("No products exist");
        }

        return Ok(product.ToDto());
    }
    [HttpGet("out-of-stock")]
    public async Task<ActionResult<IEnumerable<OutOfStockProductDto>>> GetOutOfStockProducts()
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

        return Ok(products);
    }
    [HttpGet("category-stats")]
    public async Task<ActionResult<IEnumerable<CategoryStatsDto>>> GetCategoryStats()
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

        return Ok(stats);
    }
}