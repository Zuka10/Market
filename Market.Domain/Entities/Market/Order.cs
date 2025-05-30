using Market.Domain.Entities.Auth;
using Market.Domain.Entities.Common;
using Market.Domain.Enums;

namespace Market.Domain.Entities.Market;

public class Order : AuditableEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TotalCommission { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public long LocationId { get; set; }
    public long? DiscountId { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public long UserId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Location? Location { get; set; }
    public virtual Discount? Discount { get; set; }
    public virtual User? User { get; set; }
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = [];
    public virtual ICollection<Payment> Payments { get; set; } = [];

    // Computed properties
    public decimal TotalPaid => Payments?.Where(p => p.Status == Enums.PaymentStatus.Completed).Sum(p => p.Amount) ?? 0;
    public decimal AmountDue => Total - TotalPaid;
    public bool IsPaid => AmountDue <= 0;
    public int ItemCount => OrderDetails?.Sum(od => (int)od.Quantity) ?? 0;
    public int LineItemCount => OrderDetails?.Count ?? 0;
    public decimal TotalProfit => OrderDetails?.Sum(od => od.Profit ?? 0) ?? 0;
    public PaymentStatus PaymentStatus => (IsPaid, AmountDue) switch
    {
        (true, _) => PaymentStatus.Completed,
        (false, var amount) when amount < Total => PaymentStatus.Pending,
        _ => PaymentStatus.Failed
    };
}