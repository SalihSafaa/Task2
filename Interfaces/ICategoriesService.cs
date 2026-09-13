namespace ProductCatalogApi;

public interface ICategoriesService
{
    public Task<Result<IEnumerable<CategoryDto>>> GetCategoriesAsync();
    public Task<Result<CategoryDto?>> GetCategoryAsync(int id);
    public Task<Result<IEnumerable<ProductDto>>> GetProductsByCategoryAsync(int id);
    public Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
    public Task<Result<bool>> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto);
    public Task<Result<bool>> DeleteCategoryAsync(int id);
}