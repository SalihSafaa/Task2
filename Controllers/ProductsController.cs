namespace ProductCatalogApi;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return await _context.Products.ToListAsync();
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return product;
    }
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)

    {

        var category = await _context.Categories.AnyAsync(c => c.Id == product.CategoryId);
        if (!category)
        {
            return BadRequest("Invalid category ID");
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }
        var category = await _context.Categories.AnyAsync(c => c.Id == product.CategoryId);
        if (!category)
        {
            return BadRequest("Invalid category ID");
        }

        _context.Entry(product).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            if (!ProductExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }

}