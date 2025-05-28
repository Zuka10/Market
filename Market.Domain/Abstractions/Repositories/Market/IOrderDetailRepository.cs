using Market.Domain.Entities.Market;

namespace Market.Domain.Abstractions.Repositories.Market;

public interface IOrderDetailRepository : IGenericRepository<OrderDetail>
{
    Task<IEnumerable<OrderDetail>> GetByOrderAsync(long orderId);
    Task<IEnumerable<OrderDetail>> GetByProductAsync(long productId);
    Task<IEnumerable<OrderDetail>> GetOrderDetailsWithProductsAsync(long orderId);
    Task<decimal> GetTotalProfitByOrderAsync(long orderId);
}