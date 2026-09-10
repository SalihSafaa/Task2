using Microsoft.EntityFrameworkCore;

namespace ProductCatalogApi;

public class ProductsService : IProductsService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductsService> _logger;

    public ProductsService(AppDbContext context, ILogger<ProductsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PaginationResponseDto<ProductDto>>> GetProductsAsync(PaginationRequestDto query)
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

        return Result<PaginationResponseDto<ProductDto>>.Success(response);
    }

    public async Task<Result<ProductDto?>> GetProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Attempted to retrieve a non-existent product with ID: {ProductId}", id);
            return Result<ProductDto?>.Failure(ErrorType.NotFound, $"Product with ID {id} not found.");
        }
        return Result<ProductDto?>.Success(product.ToDto());
    }

    public async Task<Result<ProductDto>> CreateProductAsync(CreateProductDto createProductDto)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == createProductDto.CategoryId);
        if (!categoryExists)
        {
            _logger.LogWarning("Attempted to create a product with an invalid category ID: {CategoryId}", createProductDto.CategoryId);
            return Result<ProductDto>.Failure(ErrorType.Validation, "Invalid category ID");
        }

        var product = createProductDto.ToEntity();
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Product created with ID: {ProductId} and Category ID: {CategoryId}", product.Id, product.CategoryId);
        return Result<ProductDto>.Success(product.ToDto());
    }

    public async Task<Result<bool>> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Attempted to update a non-existent product with ID: {ProductId}", id);
            return Result<bool>.Failure(ErrorType.NotFound, $"Product with ID {id} not found.");
        }

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == updateProductDto.CategoryId);
        if (!categoryExists)
        {
            _logger.LogWarning("Attempted to update product ID: {ProductId} with an invalid category ID: {CategoryId}", id, updateProductDto.CategoryId);
            return Result<bool>.Failure(ErrorType.Validation, "Category not found");
        }

        updateProductDto.UpdateEntity(product);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Product with ID: {ProductId} updated successfully", id);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Attempted to delete a non-existent product with ID: {ProductId}", id);
            return Result<bool>.Failure(ErrorType.NotFound, $"Product with ID {id} not found.");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Product with ID: {ProductId} deleted successfully", id);
        return Result<bool>.Success(true);
    }
}