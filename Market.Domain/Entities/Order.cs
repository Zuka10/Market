namespace Market.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = null!;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public ICollection<OrderDetail> OrderDetails { get; set; } = [];
    public decimal TotalPrice => OrderDetails.Sum(od => od.UnitPrice * od.Quantity);
}