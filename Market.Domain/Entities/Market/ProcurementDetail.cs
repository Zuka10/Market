using Market.Domain.Entities.Common;

namespace Market.Domain.Entities.Market;

public class ProcurementDetail : BaseEntity
{
    public long ProcurementId { get; set; }
    public long ProductId { get; set; }
    public decimal PurchasePrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    // Navigation properties
    public virtual Procurement Procurement { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;

    // Computed properties
    public decimal CalculatedLineTotal => PurchasePrice * Quantity;
    public decimal PurchasePricePerUnit => PurchasePrice;
    public decimal? PotentialProfitPerUnit => Product?.Price - PurchasePrice;
    public decimal? PotentialTotalProfit => PotentialProfitPerUnit * Quantity;
}