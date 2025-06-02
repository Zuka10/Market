namespace Market.Domain.Filters;

public class ProcurementFilterParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? ReferenceNo { get; set; }
    public long? VendorId { get; set; }
    public string? VendorName { get; set; }
    public long? LocationId { get; set; }
    public string? LocationName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Notes { get; set; }
    public string? SortBy { get; set; } = "procurementdate";
    public string? SortDirection { get; set; } = "desc";
}