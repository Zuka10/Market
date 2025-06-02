namespace Market.Domain.Filters;

public class ProductFilterParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsAvailable { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinStock { get; set; }
    public int? MaxStock { get; set; }
    public int? LowStockThreshold { get; set; }
    public bool? IsOutOfStock { get; set; }
    public bool? IsLowStock { get; set; }
    public string? Unit { get; set; }
    public long? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public long? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string? SortBy { get; set; } = "name";
    public string? SortDirection { get; set; } = "asc";
}