using Market.Domain.Enums;

namespace Market.Domain.Filters;

public class PaymentFilterParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public long? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public PaymentStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public long? UserId { get; set; }
    public long? LocationId { get; set; }
    public string? CustomerName { get; set; }
    public string? SortBy { get; set; } = "paymentdate";
    public string? SortDirection { get; set; } = "desc";
}