using Market.Domain.Entities.Common;

namespace Market.Domain.Entities.Market;

public class Product : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int InStock { get; set; } = 0;
    public string Unit { get; set; } = "piece";
    public long LocationId { get; set; }
    public long CategoryId { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Navigation properties
    public virtual Location? Location { get; set; }
    public virtual Category? Category { get; set; }
    public virtual ICollection<ProcurementDetail> ProcurementDetails { get; set; } = [];
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = [];

    // Computed properties
    public bool IsInStock => InStock > 0;
    public string DisplayName => $"{Name} ({Price:C} per {Unit})";
    public decimal TotalValue => Price * InStock;
    public string StockStatus => InStock switch
    {
        0 => "Out of Stock",
        < 10 => "Low Stock",
        _ => "In Stock"
    };
}