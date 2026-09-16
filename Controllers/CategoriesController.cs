using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductCatalogApi;

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
            return this.ToActionResult(result);
        return Ok(result.Value);
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var result = await _service.GetCategoryAsync(id);
        if (!result.IsSuccess)
            return this.ToActionResult(result);

        return Ok(result.Value);
    }
    [HttpGet("{id:int}/products")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int id)
    {
        var result = await _service.GetProductsByCategoryAsync(id);
        if (!result.IsSuccess)
            return this.ToActionResult(result);

        return Ok(result.Value);
    }
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createCategoryDto)
    {
        var result = await _service.CreateCategoryAsync(createCategoryDto);
        if (!result.IsSuccess)
            return this.ToActionResult(result);
        return CreatedAtAction(nameof(GetCategory), new { id = result.Value!.Id }, result.Value);
    }
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto updateCategoryDto)
    {
        var result = await _service.UpdateCategoryAsync(id, updateCategoryDto);
        if (!result.IsSuccess)
            return this.ToActionResult(result);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _service.DeleteCategoryAsync(id);
        if (!result.IsSuccess)
            return this.ToActionResult(result);
        return NoContent();
    }
}