using Market.Domain.Entities.Market;
using Market.Domain.Enums;

namespace Market.Domain.Abstractions.Repositories.Market;

public interface IPaymentRepository : IGenericRepository<Payment>
{
    Task<IEnumerable<Payment>> GetByOrderAsync(long orderId);
    Task<IEnumerable<Payment>> GetPaymentsByMethodAsync(PaymentMethod method);
    Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status);
    Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetCompletedPaymentsByOrderAsync(long orderId);
}