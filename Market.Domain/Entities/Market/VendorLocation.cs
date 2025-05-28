using Market.Domain.Entities.Common;

namespace Market.Domain.Entities.Market;

public class VendorLocation : AuditableEntity
{
    public long VendorId { get; set; }
    public long LocationId { get; set; }
    public string? StallNumber { get; set; }
    public decimal? RentAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Navigation properties
    public virtual Vendor? Vendor { get; set; }
    public virtual Location? Location { get; set; }

    // Computed properties
    public bool IsCurrentlyActive => IsActive && StartDate <= DateTime.UtcNow &&
                                   (EndDate == null || EndDate > DateTime.UtcNow);
    public int DaysActive => (DateTime.UtcNow - StartDate).Days;
    public string DisplayName => $"{Vendor?.Name} @ {Location?.Name}" +
                               (string.IsNullOrEmpty(StallNumber) ? "" : $" (Stall: {StallNumber})");
}