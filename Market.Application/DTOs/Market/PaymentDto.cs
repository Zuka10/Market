namespace Market.Application.DTOs.Market;

public class PaymentDto
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string UpdatedByName { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public string DisplayInfo { get; set; } = string.Empty;
}