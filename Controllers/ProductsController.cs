namespace ProductCatalogApi;

using Microsoft.AspNetCore.Mvc;
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
        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var result = await _service.GetProductAsync(id);
        if (!result.IsSuccess)
            return NotFound(result.ErrorMessage);

        return Ok(result.Value);
    }
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
        var result = await _service.CreateProductAsync(createProductDto);
        if (!result.IsSuccess)
        {
            return result.ErrorType switch
            {
                ErrorType.NotFound => (ActionResult<ProductDto>)NotFound(result.ErrorMessage),
                ErrorType.Validation => (ActionResult<ProductDto>)BadRequest(result.ErrorMessage),
                ErrorType.Conflict => (ActionResult<ProductDto>)Conflict(result.ErrorMessage),
                ErrorType.UnAuthorized => (ActionResult<ProductDto>)Unauthorized(result.ErrorMessage),
                _ => (ActionResult<ProductDto>)StatusCode(500, "An unexpected error occurred."),
            };
        }

        return CreatedAtAction(nameof(GetProduct), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateProductDto)
    {
        var result = await _service.UpdateProductAsync(id, updateProductDto);
        if (!result.IsSuccess)
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

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _service.DeleteProductAsync(id);
        if (!result.IsSuccess)
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

        return NoContent();
    }
}