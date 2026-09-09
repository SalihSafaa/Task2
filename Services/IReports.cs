namespace ProductCatalogApi;

public interface IReports
{
    public Task<ReportsDto> GetInventoryValueAsync();
    public Task<ProductDto?> GetMostExpensiveProductAsync();
    public Task<IEnumerable<OutOfStockProductDto>> GetOutOfStockProductsAsync();
    public Task<IEnumerable<CategoryStatsDto>> GetCategoryStatsAsync();
}
