namespace Market.Application.DTOs.Market;

public class ProcurementDetailDto
{
    public long Id { get; set; }
    public long ProcurementId { get; set; }
    public string? ProcurementReferenceNo { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public decimal CalculatedLineTotal { get; set; }
    public decimal PurchasePricePerUnit { get; set; }
    public decimal? PotentialProfitPerUnit { get; set; }
    public decimal? PotentialTotalProfit { get; set; }
}