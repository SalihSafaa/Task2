namespace ProductCatalogApi;

using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoriesService _service;

    public CategoriesController(ICategoriesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var result = await _service.GetCategoriesAsync();
        if (!result.IsSuccess)
            return NotFound(result.ErrorMessage);
        return Ok(result.Value);
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var result = await _service.GetCategoryAsync(id);
        if (!result.IsSuccess)
            return NotFound(result.ErrorMessage);

        return Ok(result.Value);
    }
    [HttpGet("{id:int}/products")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int id)
    {
        var result = await _service.GetProductsByCategoryAsync(id);
        if (!result.IsSuccess)
            return NotFound(result.ErrorMessage);

        return Ok(result.Value);
    }
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createCategoryDto)
    {
        var result = await _service.CreateCategoryAsync(createCategoryDto);
        if (!result.IsSuccess)
        {
            return result.ErrorType switch
            {
                ErrorType.NotFound => (ActionResult<CategoryDto>)NotFound(result.ErrorMessage),
                ErrorType.Validation => (ActionResult<CategoryDto>)BadRequest(result.ErrorMessage),
                ErrorType.Conflict => (ActionResult<CategoryDto>)Conflict(result.ErrorMessage),
                ErrorType.UnAuthorized => (ActionResult<CategoryDto>)Unauthorized(result.ErrorMessage),
                _ => (ActionResult<CategoryDto>)StatusCode(500, "An unexpected error occurred."),
            };
        }
        return CreatedAtAction(nameof(GetCategory), new { id = result.Value!.Id }, result.Value);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto updateCategoryDto)
    {
        var result = await _service.UpdateCategoryAsync(id, updateCategoryDto);
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
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _service.DeleteCategoryAsync(id);
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