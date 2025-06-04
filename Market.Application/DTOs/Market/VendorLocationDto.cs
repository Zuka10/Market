namespace Market.Application.DTOs.Market;

public class VendorLocationDto
{
    public long Id { get; set; }
    public long VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public long LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string? StallNumber { get; set; }
    public decimal? RentAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string UpdatedByName { get; set; } = string.Empty;
    public bool IsCurrentlyActive { get; set; }
    public int DaysActive { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}