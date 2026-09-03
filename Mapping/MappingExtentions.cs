namespace ProductCatalogApi;

public static class MappingExtentions
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt,
            CategoryId = product.CategoryId
        };
    }
    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description ?? string.Empty
        };
    }
    public static Product ToEntity(this CreateProductDto createProductDto)
    {
        return new Product
        {
            Name = createProductDto.Name,
            Price = createProductDto.Price,
            Stock = createProductDto.Stock,
            CategoryId = createProductDto.CategoryId
        };
    }
    public static Category ToEntity(this CreateCategoryDto createCategoryDto)
    {
        return new Category
        {
            Name = createCategoryDto.Name,
            Description = createCategoryDto.Description ?? string.Empty
        };
    }
    public static void UpdateEntity(this UpdateProductDto newproductDto, Product originalProduct)
    {
        originalProduct.Name = newproductDto.Name;
        originalProduct.Price = newproductDto.Price;
        originalProduct.Stock = newproductDto.Stock;
        originalProduct.CategoryId = newproductDto.CategoryId;
    }

    public static void UpdateEntity(this UpdateCategoryDto newCategoryDto, Category originalCategory)
    {
        originalCategory.Name = newCategoryDto.Name;
        originalCategory.Description = newCategoryDto.Description ?? string.Empty;
    }
}