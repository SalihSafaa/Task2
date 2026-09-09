namespace ProductCatalogApi;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(AppDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }
    [HttpGet] // api/products?pageNumber=1&pageSize=10&search=&categoryId=&minPrice=
    public async Task<ActionResult<PaginationResponseDto<ProductDto>>> GetProducts([FromQuery] PaginationRequestDto query)
    {
        var products = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
            products = products.Where(p => p.Name.Contains(query.Search));

        if (query.CategoryId.HasValue)
            products = products.Where(p => p.CategoryId == query.CategoryId.Value);

        if (query.MinPrice.HasValue)
            products = products.Where(p => p.Price >= query.MinPrice.Value);

        var totalCount = await products.CountAsync();


        var items = await products
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => p.ToDto())
            .ToListAsync();

        var response = new PaginationResponseDto<ProductDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return product.ToDto();
    }
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == createProductDto.CategoryId);
        if (!categoryExists)
        {
            _logger.LogWarning("Attempted to create a product with an invalid category ID: {CategoryId}", createProductDto.CategoryId);
            return BadRequest("Invalid category ID");
        }

        var product = createProductDto.ToEntity();
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Product created with ID: {ProductId} and Category ID: {CategoryId}", product.Id, product.CategoryId);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateProductDto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Attempted to update a non-existent product with ID: {ProductId}", id);
            return NotFound();
        }

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == updateProductDto.CategoryId);
        if (!categoryExists)
        {
            _logger.LogWarning("Attempted to update product ID: {ProductId} with an invalid category ID: {CategoryId}", id, updateProductDto.CategoryId);
            return BadRequest("category not found");
        }

        updateProductDto.UpdateEntity(product);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Product with ID: {ProductId} updated successfully", id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Attempted to delete a non-existent product with ID: {ProductId}", id);
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Product with ID: {ProductId} deleted successfully", id);
        return NoContent();
    }
}