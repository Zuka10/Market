using Market.Domain.Entities.Common;

namespace Market.Domain.Entities.Market;

public class Procurement : AuditableEntity
{
    public long VendorId { get; set; }
    public long LocationId { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime ProcurementDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Vendor? Vendor { get; set; }
    public virtual Location? Location { get; set; }
    public virtual ICollection<ProcurementDetail> ProcurementDetails { get; set; } = new List<ProcurementDetail>();

    // Computed properties
    public int ItemCount => ProcurementDetails?.Sum(pd => pd.Quantity) ?? 0;
    public int LineItemCount => ProcurementDetails?.Count ?? 0;
    public decimal CalculatedTotal => ProcurementDetails?.Sum(pd => pd.LineTotal) ?? 0;
    public string DisplayName => $"Procurement {ReferenceNo ?? Id.ToString()} - {Vendor?.Name}";
}