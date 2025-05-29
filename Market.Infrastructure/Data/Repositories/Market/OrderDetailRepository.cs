using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Market;
using Market.Infrastructure.Constants;

namespace Market.Infrastructure.Data.Repositories.Market;

public class OrderDetailRepository(IDbConnectionFactory connectionFactory) :
    GenericRepository<OrderDetail>(connectionFactory, DatabaseConstants.Tables.Market.OrderDetail, DatabaseConstants.Schemas.Market),
    IOrderDetailRepository
{
    public async Task<IEnumerable<OrderDetail>> GetByOrderAsync(long orderId)
    {
        using var orderConnection = await _connectionFactory.CreateConnectionAsync();
        var orderCheckSql = "SELECT COUNT(1) FROM market.[Order] WHERE Id = @OrderId";
        var orderExists = await orderConnection.QuerySingleAsync<int>(orderCheckSql, new { OrderId = orderId });
        if (orderExists == 0)
        {
            throw new KeyNotFoundException($"Order with ID '{orderId}' was not found.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE OrderId = @OrderId";
        return await connection.QueryAsync<OrderDetail>(sql, new { OrderId = orderId });
    }

    public async Task<IEnumerable<OrderDetail>> GetByProductAsync(long productId)
    {
        using var productConnection = await _connectionFactory.CreateConnectionAsync();
        var productCheckSql = "SELECT COUNT(1) FROM market.Product WHERE Id = @ProductId";
        var productExists = await productConnection.QuerySingleAsync<int>(productCheckSql, new { ProductId = productId });
        if (productExists == 0)
        {
            throw new KeyNotFoundException($"Product with ID '{productId}' was not found.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE ProductId = @ProductId";
        return await connection.QueryAsync<OrderDetail>(sql, new { ProductId = productId });
    }

    public async Task<IEnumerable<OrderDetail>> GetOrderDetailsWithProductsAsync(long orderId)
    {
        using var orderConnection = await _connectionFactory.CreateConnectionAsync();
        var orderCheckSql = "SELECT COUNT(1) FROM market.[Order] WHERE Id = @OrderId";
        var orderExists = await orderConnection.QuerySingleAsync<int>(orderCheckSql, new { OrderId = orderId });
        if (orderExists == 0)
        {
            throw new KeyNotFoundException($"Order with ID '{orderId}' was not found.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT od.*, p.Id, p.Name, p.Description, p.Unit, p.Price
                FROM market.OrderDetail od
                INNER JOIN market.Product p ON od.ProductId = p.Id
                WHERE od.OrderId = @OrderId";
        var result = await connection.QueryAsync<OrderDetail, Product, OrderDetail>(
            sql,
            (orderDetail, product) =>
            {
                orderDetail.Product = product;
                return orderDetail;
            },
            new { OrderId = orderId },
            splitOn: "Id");
        return result;
    }

    public async Task<decimal> GetTotalProfitByOrderAsync(long orderId)
    {
        using var orderConnection = await _connectionFactory.CreateConnectionAsync();
        var orderCheckSql = "SELECT COUNT(1) FROM market.[Order] WHERE Id = @OrderId";
        var orderExists = await orderConnection.QuerySingleAsync<int>(orderCheckSql, new { OrderId = orderId });
        if (orderExists == 0)
        {
            throw new KeyNotFoundException($"Order with ID '{orderId}' was not found.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT ISNULL(SUM(Profit), 0) FROM {FullTableName} WHERE OrderId = @OrderId";
        return await connection.QuerySingleAsync<decimal>(sql, new { OrderId = orderId });
    }

    public override async Task UpdateAsync(OrderDetail entity)
    {
        await ValidateForeignKeys(entity);

        ValidateOrderDetailRules(entity);

        await ValidateOrderEditability(entity.OrderId);

        await base.UpdateAsync(entity);
    }

    public override async Task<OrderDetail> AddAsync(OrderDetail entity)
    {
        await ValidateForeignKeys(entity);

        ValidateOrderDetailRules(entity);

        await ValidateOrderEditability(entity.OrderId);

        await ValidateProductAvailability(entity);

        return await base.AddAsync(entity);
    }

    public override async Task DeleteAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var orderIdSql = $"SELECT OrderId FROM {FullTableName} WHERE Id = @Id";
        var orderId = await connection.QuerySingleAsync<long>(orderIdSql, new { Id = id });

        await ValidateOrderEditability(orderId);

        await base.DeleteAsync(id);
    }

    private async Task ValidateForeignKeys(OrderDetail entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var orderCheckSql = "SELECT COUNT(1) FROM market.[Order] WHERE Id = @OrderId";
        var orderExists = await connection.QuerySingleAsync<int>(orderCheckSql, new { OrderId = entity.OrderId });
        if (orderExists == 0)
        {
            throw new ArgumentException($"Order with ID '{entity.OrderId}' does not exist.");
        }

        var productCheckSql = "SELECT COUNT(1) FROM market.Product WHERE Id = @ProductId AND IsAvailable = 1";
        var productExists = await connection.QuerySingleAsync<int>(productCheckSql, new { ProductId = entity.ProductId });
        if (productExists == 0)
        {
            throw new ArgumentException($"Product with ID '{entity.ProductId}' does not exist or is not available.");
        }
    }

    private static void ValidateOrderDetailRules(OrderDetail entity)
    {
        if (entity.Quantity <= 0)
        {
            throw new ArgumentException("Order detail quantity must be greater than zero.");
        }

        if (entity.UnitPrice < 0)
        {
            throw new ArgumentException("Order detail unit price cannot be negative.");
        }

        if (entity.LineTotal < 0)
        {
            throw new ArgumentException("Order detail line total cannot be negative.");
        }

        if (entity.CostPrice < 0)
        {
            throw new ArgumentException("Order detail cost price cannot be negative.");
        }

        var expectedLineTotal = entity.Quantity * entity.UnitPrice;
        if (Math.Abs(entity.LineTotal - expectedLineTotal) > 0.01m)
        {
            throw new ArgumentException($"Line total ({entity.LineTotal:C}) does not match quantity ({entity.Quantity}) × unit price ({entity.UnitPrice:C}) = {expectedLineTotal:C}.");
        }

        var expectedProfit = entity.LineTotal - (entity.Quantity * entity.CostPrice);
        if (Math.Abs((decimal)(entity.Profit! - expectedProfit!)) > 0.01m)
        {
            throw new ArgumentException($"Profit calculation is incorrect. Expected: {expectedProfit:C}, Actual: {entity.Profit:C}.");
        }
    }

    private async Task ValidateOrderEditability(long orderId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var orderStatusSql = "SELECT Status FROM market.[Order] WHERE Id = @OrderId";
        var orderStatus = await connection.QuerySingleAsync<int>(orderStatusSql, new { OrderId = orderId });

        if (orderStatus >= 3) // Completed or Cancelled
        {
            throw new InvalidOperationException($"Cannot modify order details for order with ID '{orderId}' because the order status is '{(Domain.Enums.OrderStatus)orderStatus}'. Only Pending, Confirmed, or InProgress orders can be modified.");
        }
    }

    private async Task ValidateProductAvailability(OrderDetail entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var stockCheckSql = "SELECT InStock FROM market.Product WHERE Id = @ProductId";
        var inStock = await connection.QuerySingleAsync<int>(stockCheckSql, new { ProductId = entity.ProductId });

        if (inStock < entity.Quantity)
        {
            throw new ArgumentException($"Insufficient stock for product ID '{entity.ProductId}'. Available: {inStock}, Requested: {entity.Quantity}.");
        }

        var duplicateCheckSql = $"SELECT COUNT(1) FROM {FullTableName} WHERE OrderId = @OrderId AND ProductId = @ProductId";
        var duplicateExists = await connection.QuerySingleAsync<int>(duplicateCheckSql,
            new { OrderId = entity.OrderId, ProductId = entity.ProductId });

        if (duplicateExists > 0)
        {
            throw new ArgumentException($"Product with ID '{entity.ProductId}' is already in order '{entity.OrderId}'. Update the existing order detail instead of adding a new one.");
        }
    }

    protected override string GenerateInsertQuery()
    {
        return @"
                INSERT INTO market.OrderDetail (OrderId, ProductId, Quantity, UnitPrice, LineTotal, CostPrice, Profit)
                VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @LineTotal, @CostPrice, @Profit);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
    }

    protected override string GenerateUpdateQuery()
    {
        return @"
                UPDATE market.OrderDetail 
                SET OrderId = @OrderId, 
                    ProductId = @ProductId, 
                    Quantity = @Quantity, 
                    UnitPrice = @UnitPrice, 
                    LineTotal = @LineTotal, 
                    CostPrice = @CostPrice, 
                    Profit = @Profit
                WHERE Id = @Id";
    }
}