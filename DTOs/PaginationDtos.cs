using System.ComponentModel.DataAnnotations;

namespace ProductCatalogApi;

public class PaginationRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }
    public int? CategoryId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinPrice { get; set; }
}
public class PaginationResponseDto<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}