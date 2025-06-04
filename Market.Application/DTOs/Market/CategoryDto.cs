namespace Market.Application.DTOs.Market;

public class CategoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string UpdatedByName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int AvailableProductCount { get; set; }
    public List<ProductDto> Products { get; set; } = [];
}