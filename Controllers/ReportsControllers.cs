using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductCatalogApi;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _service;

    public ReportsController(IReportsService service)
    {
        _service = service;
    }

    [HttpGet("inventory-value")]
    public async Task<ActionResult<ReportsDto>> GetInventoryValue()
    {
        var result = await _service.GetInventoryValueAsync();
        if (!result.IsSuccess)
            return this.ToActionResult(result);

        return Ok(result.Value);
    }
    [HttpGet("most-expensive")]
    public async Task<ActionResult<ProductDto>> GetMostExpensiveProduct()
    {
        var result = await _service.GetMostExpensiveProductAsync();
        if (!result.IsSuccess)
            return this.ToActionResult(result);

        return Ok(result.Value);
    }
    [HttpGet("out-of-stock")]
    public async Task<ActionResult<IEnumerable<OutOfStockProductDto>>> GetOutOfStockProducts()
    {
        var result = await _service.GetOutOfStockProductsAsync();
        if (!result.IsSuccess)
            return this.ToActionResult(result);

        return Ok(result.Value);
    }
    [HttpGet("category-stats")]
    public async Task<ActionResult<IEnumerable<CategoryStatsDto>>> GetCategoryStats()
    {
        var result = await _service.GetCategoryStatsAsync();
        if (!result.IsSuccess)
            return this.ToActionResult(result);

        return Ok(result.Value);
    }
}