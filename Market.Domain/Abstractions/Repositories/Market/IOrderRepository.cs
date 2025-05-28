using Market.Domain.Entities.Market;
using Market.Domain.Enums;

namespace Market.Domain.Abstractions.Repositories.Market;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<Order?> GetOrderWithDetailsAsync(long id);
    Task<IEnumerable<Order>> GetOrdersWithDetailsAsync();
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
    Task<IEnumerable<Order>> GetOrdersByLocationAsync(long locationId);
    Task<IEnumerable<Order>> GetOrdersByUserAsync(long userId);
    Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedOrdersAsync(int page, int pageSize, OrderStatus? status = null);
    Task<decimal> GetTotalSalesByLocationAsync(long locationId, DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<bool> IsOrderNumberExistsAsync(string orderNumber);
}