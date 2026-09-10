namespace ProductCatalogApi;

public interface IProductsService
{
    public Task<Result<PaginationResponseDto<ProductDto>>> GetProductsAsync(PaginationRequestDto query);
    public Task<Result<ProductDto?>> GetProductAsync(int id);
    public Task<Result<ProductDto>> CreateProductAsync(CreateProductDto createProductDto);
    public Task<Result<bool>> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
    public Task<Result<bool>> DeleteProductAsync(int id);
}
