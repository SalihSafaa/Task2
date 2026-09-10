using Microsoft.EntityFrameworkCore;

namespace ProductCatalogApi;

public class CategoriesService : ICategoriesService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoriesService> _logger;

    public CategoriesService(AppDbContext context, ILogger<CategoriesService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<Result<IEnumerable<CategoryDto>>> GetCategoriesAsync()
    {
        var categories = await _context.Categories.Select(c => c.ToDto()).ToListAsync();
        return Result<IEnumerable<CategoryDto>>.Success(categories);
    }
    public async Task<Result<CategoryDto?>> GetCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            _logger.LogWarning("Attempted to retrieve a non-existent category with ID: {CategoryId}", id);
            return Result<CategoryDto?>.Failure(ErrorType.NotFound, $"Category with ID {id} not found.");
        }
        return Result<CategoryDto?>.Success(category.ToDto());
    }
    public async Task<Result<IEnumerable<ProductDto>>> GetProductsByCategoryAsync(int id)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == id);
        if (!categoryExists)
        {
            _logger.LogWarning("Attempted to retrieve products for a non-existent category with ID: {CategoryId}", id);
            return Result<IEnumerable<ProductDto>>.Failure(ErrorType.NotFound, $"Category with ID {id} not found.");
        }

        var products = await _context.Products.Where(p => p.CategoryId == id).Select(p => p.ToDto()).ToListAsync();
        return Result<IEnumerable<ProductDto>>.Success(products);
    }
    public async Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
    {
        var category = createCategoryDto.ToEntity();
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Category created with ID: {CategoryId}", category.Id);

        return Result<CategoryDto>.Success(category.ToDto());
    }
    public async Task<Result<bool>> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            _logger.LogWarning("Attempted to update a non-existent category with ID: {CategoryId}", id);
            return Result<bool>.Failure(ErrorType.NotFound, $"Category with ID {id} not found.");
        }

        _context.Entry(category).State = EntityState.Modified;
        updateCategoryDto.UpdateEntity(category);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Category with ID: {CategoryId} updated successfully", id);

        return Result<bool>.Success(true);
    }
    public async Task<Result<bool>> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            _logger.LogWarning("Attempted to delete a non-existent category with ID: {CategoryId}", id);
            return Result<bool>.Failure(ErrorType.NotFound, $"Category with ID {id} not found.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category with ID: {CategoryId} deleted successfully", id);

        return Result<bool>.Success(true);
    }

}