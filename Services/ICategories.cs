namespace ProductCatalogApi;

public interface ICategories
{
    public Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
    public Task<CategoryDto?> GetCategoryAsync(int id);
    public Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int id);
    public Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
    public Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto);
    public Task<bool> DeleteCategoryAsync(int id);
}