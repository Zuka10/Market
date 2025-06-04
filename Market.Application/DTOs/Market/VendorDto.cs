namespace Market.Application.DTOs.Market;

public class VendorDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactPersonName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal CommissionRate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string UpdatedByName { get; set; } = string.Empty;
    public decimal CommissionRatePercentage { get; set; }
    public int LocationCount { get; set; }
    public List<VendorLocationDto> VendorLocations { get; set; } = [];
}