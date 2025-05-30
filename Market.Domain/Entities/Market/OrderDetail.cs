using Market.Domain.Entities.Common;

namespace Market.Domain.Entities.Market;

public class OrderDetail : BaseEntity
{
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public long Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? Profit { get; set; }

    // Navigation properties
    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }

    // Computed properties
    public decimal CalculatedLineTotal => UnitPrice * Quantity;
    public decimal? ProfitMargin => CostPrice.HasValue && CostPrice > 0 ?
                                   (UnitPrice - CostPrice.Value) / CostPrice.Value * 100 : null;
    public decimal? ProfitPerUnit => UnitPrice - (CostPrice ?? 0);
}