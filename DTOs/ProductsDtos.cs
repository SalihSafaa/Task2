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
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Range(0.1, 1000000)]//1M
    public decimal Price { get; set; }
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    public int CategoryId { get; set; }
}
public class UpdateProductDto //validating data from user
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0.1, 1000000)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    public int CategoryId { get; set; }
}