namespace Market.Application.DTOs.Market;

public class DiscountDto
{
    public long Id { get; set; }
    public string DiscountCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Percentage { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public long? LocationId { get; set; }
    public string? LocationName { get; set; }
    public long? VendorId { get; set; }
    public string? VendorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string UpdatedByName { get; set; } = string.Empty;
    public bool IsCurrentlyValid { get; set; }
    public decimal DiscountRateDecimal { get; set; }
    public string ValidityPeriod { get; set; } = string.Empty;
}