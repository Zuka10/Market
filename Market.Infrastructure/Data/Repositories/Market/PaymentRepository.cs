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
        using var orderConnection = await _connectionFactory.CreateConnectionAsync();
        var orderCheckSql = "SELECT COUNT(1) FROM market.[Order] WHERE Id = @OrderId";
        var orderExists = await orderConnection.QuerySingleAsync<int>(orderCheckSql, new { OrderId = orderId });
        if (orderExists == 0)
        {
            throw new KeyNotFoundException($"Order with ID '{orderId}' was not found.");
        }

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
        using var orderConnection = await _connectionFactory.CreateConnectionAsync();
        var orderCheckSql = "SELECT COUNT(1) FROM market.[Order] WHERE Id = @OrderId";
        var orderExists = await orderConnection.QuerySingleAsync<int>(orderCheckSql, new { OrderId = orderId });
        if (orderExists == 0)
        {
            throw new KeyNotFoundException($"Order with ID '{orderId}' was not found.");
        }

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

    public override async Task UpdateAsync(Payment entity)
    {
        await ValidatePaymentStatusTransition(entity);

        await ValidateForeignKeys(entity);

        await ValidatePaymentRules(entity);

        await base.UpdateAsync(entity);
    }

    public override async Task<Payment> AddAsync(Payment entity)
    {
        await ValidateForeignKeys(entity);

        await ValidatePaymentRules(entity);

        if (entity.PaymentDate > DateTime.UtcNow.AddDays(1))
        {
            throw new ArgumentException("Payment date cannot be more than 1 day in the future.");
        }

        if (entity.PaymentDate < DateTime.UtcNow.AddYears(-1))
        {
            throw new ArgumentException("Payment date cannot be more than 1 year in the past.");
        }

        return await base.AddAsync(entity);
    }

    public override async Task DeleteAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var statusCheckSql = $"SELECT Status FROM {FullTableName} WHERE Id = @Id";
        var status = await connection.QuerySingleAsync<int>(statusCheckSql, new { Id = id });

        if (status == (int)PaymentStatus.Completed)
        {
            throw new InvalidOperationException($"Cannot delete payment with ID '{id}' because it has been completed. Completed payments cannot be deleted for audit purposes.");
        }

        if (status == (int)PaymentStatus.Refunded)
        {
            throw new InvalidOperationException($"Cannot delete payment with ID '{id}' because it has been refunded. Refunded payments cannot be deleted for audit purposes.");
        }

        await base.DeleteAsync(id);
    }

    private async Task ValidateForeignKeys(Payment entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var orderCheckSql = "SELECT COUNT(1) FROM market.[Order] WHERE Id = @OrderId";
        var orderExists = await connection.QuerySingleAsync<int>(orderCheckSql, new { OrderId = entity.OrderId });
        if (orderExists == 0)
        {
            throw new ArgumentException($"Order with ID '{entity.OrderId}' does not exist.");
        }
    }

    private async Task ValidatePaymentRules(Payment entity)
    {
        if (entity.Amount <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than zero.");
        }

        if (entity.Amount > 1000000) // Reasonable upper limit
        {
            throw new ArgumentException("Payment amount cannot exceed 1,000,000.");
        }

        if (!Enum.IsDefined(typeof(PaymentMethod), entity.PaymentMethod))
        {
            throw new ArgumentException("Invalid payment method.");
        }

        if (!Enum.IsDefined(typeof(PaymentStatus), entity.Status))
        {
            throw new ArgumentException("Invalid payment status.");
        }

        // Check if total payments for order would exceed order total
        using var connection = await _connectionFactory.CreateConnectionAsync();

        // Get order total
        var orderTotalSql = "SELECT Total FROM market.[Order] WHERE Id = @OrderId";
        var orderTotal = await connection.QuerySingleAsync<decimal>(orderTotalSql, new { OrderId = entity.OrderId });

        var existingPaymentsSql = entity.Id > 0
            ? "SELECT ISNULL(SUM(Amount), 0) FROM market.Payment WHERE OrderId = @OrderId AND Status = @CompletedStatus AND Id != @PaymentId"
            : "SELECT ISNULL(SUM(Amount), 0) FROM market.Payment WHERE OrderId = @OrderId AND Status = @CompletedStatus";

        var existingPayments = await connection.QuerySingleAsync<decimal>(existingPaymentsSql, new
        {
            entity.OrderId,
            CompletedStatus = (int)PaymentStatus.Completed,
            PaymentId = entity.Id
        });

        if (entity.Status == PaymentStatus.Completed)
        {
            var totalPayments = existingPayments + entity.Amount;
            if (totalPayments > orderTotal)
            {
                throw new ArgumentException($"Total payments ({totalPayments:C}) cannot exceed order total ({orderTotal:C}).");
            }
        }
    }

    private async Task ValidatePaymentStatusTransition(Payment entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var currentStatusSql = $"SELECT Status FROM {FullTableName} WHERE Id = @Id";
        var currentStatus = await connection.QuerySingleAsync<int>(currentStatusSql, new { Id = entity.Id });

        var currentPaymentStatus = (PaymentStatus)currentStatus;
        var newPaymentStatus = entity.Status;

        // Define valid status transitions
        var validTransitions = new Dictionary<PaymentStatus, List<PaymentStatus>>
        {
            [PaymentStatus.Pending] = [PaymentStatus.Completed, PaymentStatus.Failed, PaymentStatus.Cancelled],
            [PaymentStatus.Completed] = [PaymentStatus.Refunded],
            [PaymentStatus.Failed] = [PaymentStatus.Pending], // Allow retry
            [PaymentStatus.Cancelled] = [], // No transitions from cancelled
            [PaymentStatus.Refunded] = [] // No transitions from refunded
        };

        if (validTransitions.TryGetValue(currentPaymentStatus, out List<PaymentStatus>? value) &&
            !value.Contains(newPaymentStatus))
        {
            throw new InvalidOperationException($"Invalid payment status transition from '{currentPaymentStatus}' to '{newPaymentStatus}'.");
        }
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