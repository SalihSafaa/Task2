using System.ComponentModel.DataAnnotations;

namespace ProductCatalogApi;

public class CategoryDto//showing category to the user
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
public class CreateCategoryDto //validating data from user
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
public class UpdateCategoryDto //validating data from user
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}