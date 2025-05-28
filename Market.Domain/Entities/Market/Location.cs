using Market.Domain.Entities.Common;

namespace Market.Domain.Entities.Market;

public class Location : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "Georgia";
    public string? Phone { get; set; }
    public string? OpeningHours { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<VendorLocation> VendorLocations { get; set; } = [];
    public virtual ICollection<Product> Products { get; set; } = [];
    public virtual ICollection<Discount> Discounts { get; set; } = [];
    public virtual ICollection<Order> Orders { get; set; } = [];
    public virtual ICollection<Procurement> Procurements { get; set; } = [];

    // Computed properties
    public string FullAddress => $"{Address}, {City}, {PostalCode}, {Country}".Trim(' ', ',');
    public int ActiveVendorCount => VendorLocations?.Count(vl => vl.IsActive) ?? 0;
}