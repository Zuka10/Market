using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Market;
using Market.Domain.Enums;
using Market.Infrastructure.Constants;

namespace Market.Infrastructure.Data.Repositories.Market;

public class PaymentRepository(IDbConnectionFactory connectionFactory) :
    GenericRepository<Payment>(connectionFactory, DatabaseConstants.Tables.Market.Payment, DatabaseConstants.Schemas.Market),
    IPaymentRepository
{
    public async Task<IEnumerable<Payment>> GetByOrderAsync(long orderId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE OrderId = @OrderId ORDER BY PaymentDate DESC";
        return await connection.QueryAsync<Payment>(sql, new { OrderId = orderId });
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByMethodAsync(PaymentMethod method)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE PaymentMethod = @Method ORDER BY PaymentDate DESC";
        return await connection.QueryAsync<Payment>(sql, new { Method = (int)method });
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE Status = @Status ORDER BY PaymentDate DESC";
        return await connection.QueryAsync<Payment>(sql, new { Status = (int)status });
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                SELECT * FROM {FullTableName} 
                WHERE PaymentDate >= @StartDate AND PaymentDate <= @EndDate 
                ORDER BY PaymentDate DESC";
        return await connection.QueryAsync<Payment>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<decimal> GetTotalPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var whereClause = "WHERE Status = @CompletedStatus";

        if (startDate.HasValue)
        {
            whereClause += " AND PaymentDate >= @StartDate";
        }

        if (endDate.HasValue)
        {
            whereClause += " AND PaymentDate <= @EndDate";
        }

        var sql = $"SELECT ISNULL(SUM(Amount), 0) FROM {FullTableName} {whereClause}";
        return await connection.QuerySingleAsync<decimal>(sql, new
        {
            CompletedStatus = (int)PaymentStatus.Completed,
            StartDate = startDate,
            EndDate = endDate
        });
    }

    public async Task<decimal> GetCompletedPaymentsByOrderAsync(long orderId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                SELECT ISNULL(SUM(Amount), 0) FROM {FullTableName} 
                WHERE OrderId = @OrderId AND Status = @CompletedStatus";
        return await connection.QuerySingleAsync<decimal>(sql, new
        {
            OrderId = orderId,
            CompletedStatus = (int)PaymentStatus.Completed
        });
    }

    protected override string GenerateInsertQuery()
    {
        return @"
                INSERT INTO market.Payment (OrderId, PaymentDate, Amount, PaymentMethod, Status, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
                VALUES (@OrderId, @PaymentDate, @Amount, @PaymentMethod, @Status, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
    }

    protected override string GenerateUpdateQuery()
    {
        return @"
                UPDATE market.Payment 
                SET OrderId = @OrderId, 
                    PaymentDate = @PaymentDate, 
                    Amount = @Amount, 
                    PaymentMethod = @PaymentMethod, 
                    Status = @Status, 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
    }
}