using System.ComponentModel.DataAnnotations;

namespace ProductCatalogApi;

public class ProductDto //showing product to the user
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
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