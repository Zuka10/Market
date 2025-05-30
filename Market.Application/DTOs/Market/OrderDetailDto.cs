namespace Market.Application.DTOs.Market;

public class OrderDetailDto
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public long Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? Profit { get; set; }
    public decimal CalculatedLineTotal { get; set; }
    public decimal? ProfitMargin { get; set; }
    public decimal? ProfitPerUnit { get; set; }
}