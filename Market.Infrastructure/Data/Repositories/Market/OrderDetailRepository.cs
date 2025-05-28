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
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE OrderId = @OrderId";
        return await connection.QueryAsync<OrderDetail>(sql, new { OrderId = orderId });
    }

    public async Task<IEnumerable<OrderDetail>> GetByProductAsync(long productId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE ProductId = @ProductId";
        return await connection.QueryAsync<OrderDetail>(sql, new { ProductId = productId });
    }

    public async Task<IEnumerable<OrderDetail>> GetOrderDetailsWithProductsAsync(long orderId)
    {
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
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT ISNULL(SUM(Profit), 0) FROM {FullTableName} WHERE OrderId = @OrderId";
        return await connection.QuerySingleAsync<decimal>(sql, new { OrderId = orderId });
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