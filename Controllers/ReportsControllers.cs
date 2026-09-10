namespace ProductCatalogApi;

using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/[controller]")]
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
            return MapError<ReportsDto>(result);

        return Ok(result.Value);
    }
    [HttpGet("most-expensive")]
    public async Task<ActionResult<ProductDto>> GetMostExpensiveProduct()
    {
        var result = await _service.GetMostExpensiveProductAsync();
        if (!result.IsSuccess)
            return NotFound(result.ErrorMessage);

        return Ok(result.Value);
    }
    [HttpGet("out-of-stock")]
    public async Task<ActionResult<IEnumerable<OutOfStockProductDto>>> GetOutOfStockProducts()
    {
        var result = await _service.GetOutOfStockProductsAsync();
        if (!result.IsSuccess)
            return MapError<IEnumerable<OutOfStockProductDto>>(result);

        return Ok(result.Value);
    }
    [HttpGet("category-stats")]
    public async Task<ActionResult<IEnumerable<CategoryStatsDto>>> GetCategoryStats()
    {
        var result = await _service.GetCategoryStatsAsync();
        if (!result.IsSuccess)
            return MapError<IEnumerable<CategoryStatsDto>>(result);

        return Ok(result.Value);
    }

    private ActionResult<T> MapError<T>(Result<T> result)
    {
        return result.ErrorType switch
        {
            ErrorType.NotFound => NotFound(result.ErrorMessage),
            ErrorType.Validation => BadRequest(result.ErrorMessage),
            ErrorType.Conflict => Conflict(result.ErrorMessage),
            ErrorType.UnAuthorized => Unauthorized(result.ErrorMessage),
            _ => StatusCode(500, "An unexpected error occurred."),
        };
    }
}