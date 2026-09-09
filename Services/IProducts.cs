namespace ProductCatalogApi;

public interface IProducts
{
    public Task<PaginationResponseDto<ProductDto>> GetProductsAsync(PaginationRequestDto query);
    public Task<ProductDto?> GetProductAsync(int id);
    public Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
    public Task<bool> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
    public Task<bool> DeleteProductAsync(int id);
}
