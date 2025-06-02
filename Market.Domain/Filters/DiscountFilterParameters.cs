namespace Market.Domain.Filters;

public class DiscountFilterParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? DiscountCode { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsValid { get; set; } // Currently valid (within date range)
    public decimal? MinPercentage { get; set; }
    public decimal? MaxPercentage { get; set; }
    public long? LocationId { get; set; }
    public string? LocationName { get; set; }
    public long? VendorId { get; set; }
    public string? VendorName { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
    public string? SortBy { get; set; } = "createdat";
    public string? SortDirection { get; set; } = "desc";
}