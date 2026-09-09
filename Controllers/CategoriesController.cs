namespace ProductCatalogApi;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(AppDbContext context, ILogger<CategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        return await _context.Categories.Select(c => c.ToDto()).ToListAsync();
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return category.ToDto();
    }
    [HttpGet("{id:int}/products")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int id)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == id);
        if (!categoryExists)
        {
            return NotFound();
        }

        return await _context.Products.Where(p => p.CategoryId == id).Select(p => p.ToDto()).ToListAsync();
    }
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createCategoryDto)
    {
        var category = createCategoryDto.ToEntity();
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Category created with ID: {CategoryId}", category.Id);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category.ToDto());
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto updateCategoryDto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            _logger.LogWarning("Attempted to update a non-existent category with ID: {CategoryId}", id);
            return NotFound();
        }
        _context.Entry(category).State = EntityState.Modified;

        updateCategoryDto.UpdateEntity(category);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Category with ID: {CategoryId} updated successfully", id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            _logger.LogWarning("Attempted to delete a non-existent category with ID: {CategoryId}", id);
            return NotFound();
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Category with ID: {CategoryId} deleted successfully", id);
        return NoContent();
    }

    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.Id == id);
    }
}