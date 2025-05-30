using Market.Domain.Entities.Common;

namespace Market.Domain.Entities.Market;

public class Discount : AuditableEntity
{
    public string DiscountCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Percentage { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public long? LocationId { get; set; }
    public long? VendorId { get; set; }

    // Navigation properties
    public virtual Location? Location { get; set; }
    public virtual Vendor? Vendor { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = [];

    // Computed properties
    public bool IsCurrentlyValid => IsActive &&
                                  (StartDate == null || StartDate <= DateTime.UtcNow) &&
                                  (EndDate == null || EndDate >= DateTime.UtcNow);
    public decimal DiscountRateDecimal => Percentage / 100;
    public string ValidityPeriod => $"{StartDate?.ToString("yyyy-MM-dd") ?? "No start"} to {EndDate?.ToString("yyyy-MM-dd") ?? "No end"}";
}