using Market.Domain.Enums;

namespace Market.Domain.Filters;

public class OrderFilterParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? OrderNumber { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public OrderStatus? Status { get; set; }
    public long? LocationId { get; set; }
    public long? UserId { get; set; }
    public long? DiscountId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinTotal { get; set; }
    public decimal? MaxTotal { get; set; }
    public string? SortBy { get; set; } = "orderdate";
    public string? SortDirection { get; set; } = "desc";
}