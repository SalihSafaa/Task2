using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductCatalogApi;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductsService _service;

    public ProductsController(IProductsService service)
    {
        _service = service;
    }
    [HttpGet] // api/products?pageNumber=1&pageSize=10&search=&categoryId=&minPrice=
    public async Task<ActionResult<PaginationResponseDto<ProductDto>>> GetProducts([FromQuery] PaginationRequestDto query)
    {
        var result = await _service.GetProductsAsync(query);
        if (!result.IsSuccess)
            return this.ToActionResult(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var result = await _service.GetProductAsync(id);
        if (!result.IsSuccess)
            return this.ToActionResult(result);

        return Ok(result.Value);
    }
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
        var result = await _service.CreateProductAsync(createProductDto);
        if (!result.IsSuccess)
            return this.ToActionResult(result);

        return CreatedAtAction(nameof(GetProduct), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateProductDto)
    {
        var result = await _service.UpdateProductAsync(id, updateProductDto);
        if (!result.IsSuccess)
            return this.ToActionResult(result);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _service.DeleteProductAsync(id);
        if (!result.IsSuccess)
            return this.ToActionResult(result);

        return NoContent();
    }
}