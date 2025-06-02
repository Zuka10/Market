namespace Market.Domain.Filters;

public class VendorFilterParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? Email { get; set; }
    public string? ContactPersonName { get; set; }
    public bool? IsActive { get; set; }
    public decimal? MinCommissionRate { get; set; }
    public decimal? MaxCommissionRate { get; set; }
    public long? LocationId { get; set; }
    public string? SortBy { get; set; } = "name";
    public string? SortDirection { get; set; } = "asc";
}