namespace ProductCatalogApi;

public interface IReportsService
{
    public Task<Result<ReportsDto>> GetInventoryValueAsync();
    public Task<Result<ProductDto?>> GetMostExpensiveProductAsync();
    public Task<Result<IEnumerable<OutOfStockProductDto>>> GetOutOfStockProductsAsync();
    public Task<Result<IEnumerable<CategoryStatsDto>>> GetCategoryStatsAsync();
}