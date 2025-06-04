namespace Market.Application.DTOs.Market;

public class ProductDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int InStock { get; set; }
    public string Unit { get; set; } = string.Empty;
    public long LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string UpdatedByName { get; set; } = string.Empty;
    public bool IsInStock { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public string StockStatus { get; set; } = string.Empty;
}