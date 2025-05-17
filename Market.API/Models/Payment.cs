using Market.API.Models.Enums;

namespace Market.API.Models;

public class Payment
{
    public int Id { get; set; }
    public PaymentType PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public PaymentStatus Status { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }
}