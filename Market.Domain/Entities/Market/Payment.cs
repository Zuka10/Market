using Market.Domain.Entities.Common;
using Market.Domain.Enums;

namespace Market.Domain.Entities.Market;

public class Payment : AuditableEntity
{
    public long OrderId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    // Navigation properties
    public virtual Order? Order { get; set; }

    // Computed properties
    public bool IsCompleted => Status == PaymentStatus.Completed;
    public string DisplayInfo => $"{Amount:C} via {PaymentMethod} on {PaymentDate:yyyy-MM-dd}";
}