namespace Market.Application.DTOs.Market;

public class OrderDto
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TotalCommission { get; set; }
    public string Status { get; set; } = string.Empty;
    public long LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public long? DiscountId { get; set; }
    public string? DiscountCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string UpdatedByName { get; set; } = string.Empty;
    public decimal TotalPaid { get; set; }
    public decimal AmountDue { get; set; }
    public bool IsPaid { get; set; }
    public int ItemCount { get; set; }
    public int LineItemCount { get; set; }
    public decimal TotalProfit { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public List<OrderDetailDto> OrderDetails { get; set; } = [];
    public List<PaymentDto> Payments { get; set; } = [];
}