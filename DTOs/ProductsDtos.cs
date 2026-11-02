using System.ComponentModel.DataAnnotations;

namespace ProductCatalogApi;

public class ProductDto //showing product to the user
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdate { get; set; }
    public string? CreatedBy { get; set; }
    public string? LastUpdatedBy { get; set; }
    public int CategoryId { get; set; }
}
public class CreateProductDto //validating data from user
{
    [Required(ErrorMessage = "Product name is required.")]
    [MaxLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 1000000, ErrorMessage = "Price must be between 0.01 and 1000000.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
    public int Stock { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be a valid positive number.")]
    public int CategoryId { get; set; }
}
public class UpdateProductDto //validating data from user
{
    [Required(ErrorMessage = "Product name is required.")]
    [MaxLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 1000000, ErrorMessage = "Price must be between 0.01 and 1000000.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
    public int Stock { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be a valid positive number.")]
    public int CategoryId { get; set; }
}
//the adjust products
public class AdjustStockDto //validating data from user
{
    [Range(-1000, 1000, ErrorMessage = "Delta is out of range.")]
    [CustomValidation(typeof(AdjustStockDto), nameof(ValidateDelta))]
    public int Delta { get; set; }

    [Required(ErrorMessage = "Reason is required.")]
    [MaxLength(200, ErrorMessage = "Reason cannot exceed 200 characters.")]
    public string Reason { get; set; } = string.Empty;

    public static ValidationResult? ValidateDelta(int delta, ValidationContext _) =>
        delta == 0 ? new ValidationResult("Delta cannot be zero.") : ValidationResult.Success;
}

public class StockAdjustmentResultDto //what we return
{
    public int ProductId { get; set; }
    public int NewStock { get; set; }
}