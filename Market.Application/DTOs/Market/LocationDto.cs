namespace Market.Application.DTOs.Market;

public class LocationDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? OpeningHours { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string UpdatedByName { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public int VendorCount { get; set; }
    public int ProductCount { get; set; }
    public List<VendorLocationDto> VendorLocations { get; set; } = [];
    public List<ProductDto> Products { get; set; } = [];
}