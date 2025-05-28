using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Auth;
using Market.Domain.Entities.Market;
using Market.Domain.Enums;
using Market.Infrastructure.Constants;

namespace Market.Infrastructure.Data.Repositories.Market;

public class OrderRepository(IDbConnectionFactory connectionFactory) :
    GenericRepository<Order>(connectionFactory, DatabaseConstants.Tables.Market.Order, DatabaseConstants.Schemas.Market),
    IOrderRepository
{
    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE OrderNumber = @OrderNumber";
        return await connection.QueryFirstOrDefaultAsync<Order>(sql, new { OrderNumber = orderNumber });
    }

    public async Task<Order?> GetOrderWithDetailsAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT o.*, od.Id, od.OrderId, od.ProductId, od.Quantity, od.UnitPrice, od.LineTotal, od.CostPrice, od.Profit,
                       p.Id, p.Name, p.Description, p.Unit,
                       l.Id, l.Name as LocationName, l.City,
                       u.Id, u.Username, u.FirstName, u.LastName
                FROM market.[Order] o
                LEFT JOIN market.OrderDetail od ON o.Id = od.OrderId
                LEFT JOIN market.Product p ON od.ProductId = p.Id
                INNER JOIN market.Location l ON o.LocationId = l.Id
                INNER JOIN auth.[User] u ON o.UserId = u.Id
                WHERE o.Id = @Id";

        var orderDict = new Dictionary<long, Order>();

        await connection.QueryAsync<Order, OrderDetail, Product, Location, User, Order>(
            sql,
            (order, orderDetail, product, location, user) =>
            {
                if (!orderDict.TryGetValue(order.Id, out var existingOrder))
                {
                    existingOrder = order;
                    existingOrder.OrderDetails = new List<OrderDetail>();
                    existingOrder.Location = location;
                    existingOrder.User = user;
                    orderDict.Add(order.Id, existingOrder);
                }

                if (orderDetail != null)
                {
                    orderDetail.Product = product;
                    existingOrder.OrderDetails.Add(orderDetail);
                }

                return existingOrder;
            },
            new { Id = id },
            splitOn: "Id,Id,Id,Id");

        return orderDict.Values.FirstOrDefault();
    }

    public async Task<IEnumerable<Order>> GetOrdersWithDetailsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT o.*, l.Id, l.Name as LocationName, l.City,
                       u.Id, u.Username, u.FirstName, u.LastName
                FROM market.[Order] o
                INNER JOIN market.Location l ON o.LocationId = l.Id
                INNER JOIN auth.[User] u ON o.UserId = u.Id
                ORDER BY o.OrderDate DESC";

        var result = await connection.QueryAsync<Order, Location, User, Order>(
            sql,
            (order, location, user) =>
            {
                order.Location = location;
                order.User = user;
                return order;
            },
            splitOn: "Id,Id");

        return result;
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE Status = @Status";
        return await connection.QueryAsync<Order>(sql, new { Status = (int)status });
    }

    public async Task<IEnumerable<Order>> GetOrdersByLocationAsync(long locationId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE LocationId = @LocationId ORDER BY OrderDate DESC";
        return await connection.QueryAsync<Order>(sql, new { LocationId = locationId });
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserAsync(long userId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE UserId = @UserId ORDER BY OrderDate DESC";
        return await connection.QueryAsync<Order>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                SELECT * FROM {FullTableName} 
                WHERE OrderDate >= @StartDate AND OrderDate <= @EndDate 
                ORDER BY OrderDate DESC";
        return await connection.QueryAsync<Order>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedOrdersAsync(int page, int pageSize, OrderStatus? status = null)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var whereClause = status.HasValue ? "WHERE Status = @Status" : "";
        var countSql = $"SELECT COUNT(*) FROM {FullTableName} {whereClause}";
        var totalCount = await connection.QuerySingleAsync<int>(countSql, new { Status = (int?)status });

        var offset = (page - 1) * pageSize;
        var sql = $@"
                SELECT * FROM {FullTableName} {whereClause}
                ORDER BY OrderDate DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        var orders = await connection.QueryAsync<Order>(sql,
            new { Status = (int?)status, Offset = offset, PageSize = pageSize });

        return (orders, totalCount);
    }

    public async Task<decimal> GetTotalSalesByLocationAsync(long locationId, DateTime? startDate = null, DateTime? endDate = null)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var whereClause = "WHERE LocationId = @LocationId AND Status = @CompletedStatus";

        if (startDate.HasValue)
        {
            whereClause += " AND OrderDate >= @StartDate";
        }

        if (endDate.HasValue)
        {
            whereClause += " AND OrderDate <= @EndDate";
        }

        var sql = $"SELECT ISNULL(SUM(Total), 0) FROM {FullTableName} {whereClause}";
        return await connection.QuerySingleAsync<decimal>(sql, new
        {
            LocationId = locationId,
            CompletedStatus = (int)OrderStatus.Completed,
            StartDate = startDate,
            EndDate = endDate
        });
    }

    public async Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var whereClause = "WHERE Status = @CompletedStatus";

        if (startDate.HasValue)
        {
            whereClause += " AND OrderDate >= @StartDate";
        }

        if (endDate.HasValue)
        {
            whereClause += " AND OrderDate <= @EndDate";
        }

        var sql = $"SELECT ISNULL(SUM(Total), 0) FROM {FullTableName} {whereClause}";
        return await connection.QuerySingleAsync<decimal>(sql, new
        {
            CompletedStatus = (int)OrderStatus.Completed,
            StartDate = startDate,
            EndDate = endDate
        });
    }

    public async Task<bool> IsOrderNumberExistsAsync(string orderNumber)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT COUNT(1) FROM {FullTableName} WHERE OrderNumber = @OrderNumber";
        var count = await connection.QuerySingleAsync<int>(sql, new { OrderNumber = orderNumber });
        return count > 0;
    }

    protected override string GenerateInsertQuery()
    {
        return @"
                INSERT INTO market.[Order] (OrderNumber, OrderDate, Total, SubTotal, TotalCommission, Status, LocationId, DiscountId, DiscountAmount, UserId, CustomerName, CustomerPhone, Notes, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
                VALUES (@OrderNumber, @OrderDate, @Total, @SubTotal, @TotalCommission, @Status, @LocationId, @DiscountId, @DiscountAmount, @UserId, @CustomerName, @CustomerPhone, @Notes, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
    }

    protected override string GenerateUpdateQuery()
    {
        return @"
                UPDATE market.[Order] 
                SET OrderNumber = @OrderNumber, 
                    OrderDate = @OrderDate, 
                    Total = @Total, 
                    SubTotal = @SubTotal, 
                    TotalCommission = @TotalCommission, 
                    Status = @Status, 
                    LocationId = @LocationId, 
                    DiscountId = @DiscountId, 
                    DiscountAmount = @DiscountAmount, 
                    UserId = @UserId, 
                    CustomerName = @CustomerName, 
                    CustomerPhone = @CustomerPhone, 
                    Notes = @Notes, 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
    }
}