using Market.Domain.Entities.Common;

namespace Market.Domain.Entities.Market;

public class Vendor : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string ContactPersonName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal CommissionRate { get; set; } = 10.00m;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<VendorLocation> VendorLocations { get; set; } = [];
    public virtual ICollection<Discount> Discounts { get; set; } = [];
    public virtual ICollection<Procurement> Procurements { get; set; } = [];

    // Computed properties
    public int ActiveLocationCount => VendorLocations?.Count(vl => vl.IsActive) ?? 0;
    public decimal CommissionRatePercentage => CommissionRate / 100;
}