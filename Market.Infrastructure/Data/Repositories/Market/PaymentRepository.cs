using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Auth;
using Market.Domain.Entities.Market;
using Market.Domain.Enums;
using Market.Domain.Filters;
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
    public async Task<PagedResult<Payment>> GetPaymentsAsync(PaymentFilterParameters filterParams)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var (whereClause, parameters) = BuildWhereClause(filterParams);
        var orderByClause = BuildOrderByClause(filterParams.SortBy, filterParams.SortDirection);

        // Count query
        var countSql = BuildCountQuery(whereClause);
        var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

        // Decide on pagination strategy
        if (ShouldPaginate(filterParams, totalCount))
        {
            // Use database pagination for large results
            var (offset, pageSize) = CalculatePagination(filterParams.PageNumber, filterParams.PageSize);
            parameters.Add("Offset", offset);
            parameters.Add("PageSize", pageSize);

            var dataSql = BuildDataQueryWithPagination(whereClause, orderByClause);
            var payments = await connection.QueryAsync<Payment, Order, User, Location, Payment>(
                dataSql,
                (payment, order, user, location) =>
                {
                    payment.Order = order;
                    if (order != null)
                    {
                        order.User = user;
                        order.Location = location;
                    }
                    return payment;
                },
                parameters,
                splitOn: "OrderId_Split,UserId_Split,LocationId_Split"
            );

            var paginationMetadata = CalculatePaginationMetadata(totalCount, filterParams.PageNumber, filterParams.PageSize);

            return new PagedResult<Payment>
            {
                Items = payments.ToList(),
                TotalCount = totalCount,
                Page = filterParams.PageNumber,
                PageSize = filterParams.PageSize,
                TotalPages = paginationMetadata.TotalPages,
                HasNextPage = paginationMetadata.HasNextPage,
                HasPreviousPage = paginationMetadata.HasPreviousPage
            };
        }
        else
        {
            // Return all results for small datasets
            var dataSql = BuildDataQuery(whereClause, orderByClause);
            var payments = await connection.QueryAsync<Payment, Order, User, Location, Payment>(
                dataSql,
                (payment, order, user, location) =>
                {
                    payment.Order = order;
                    if (order != null)
                    {
                        order.User = user;
                        order.Location = location;
                    }
                    return payment;
                },
                parameters,
                splitOn: "OrderId_Split,UserId_Split,LocationId_Split"
            );

            var paymentsList = payments.ToList();

            return new PagedResult<Payment>
            {
                Items = paymentsList,
                TotalCount = totalCount,
                Page = 1,
                PageSize = paymentsList.Count,
                TotalPages = 1,
                HasNextPage = false,
                HasPreviousPage = false
            };
        }
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

    private static (string WhereClause, DynamicParameters Parameters) BuildWhereClause(PaymentFilterParameters filterParams)
    {
        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
        {
            whereConditions.Add("(o.OrderNumber LIKE @SearchTerm OR o.CustomerName LIKE @SearchTerm OR o.CustomerPhone LIKE @SearchTerm OR u.Username LIKE @SearchTerm OR u.FirstName LIKE @SearchTerm OR u.LastName LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{filterParams.SearchTerm.Trim()}%");
        }

        if (filterParams.OrderId.HasValue)
        {
            whereConditions.Add("p.OrderId = @OrderId");
            parameters.Add("OrderId", filterParams.OrderId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.OrderNumber))
        {
            whereConditions.Add("o.OrderNumber LIKE @OrderNumber");
            parameters.Add("OrderNumber", $"%{filterParams.OrderNumber.Trim()}%");
        }

        if (filterParams.PaymentMethod.HasValue)
        {
            whereConditions.Add("p.PaymentMethod = @PaymentMethod");
            parameters.Add("PaymentMethod", (int)filterParams.PaymentMethod.Value);
        }

        if (filterParams.Status.HasValue)
        {
            whereConditions.Add("p.Status = @Status");
            parameters.Add("Status", (int)filterParams.Status.Value);
        }

        if (filterParams.StartDate.HasValue)
        {
            whereConditions.Add("p.PaymentDate >= @StartDate");
            parameters.Add("StartDate", filterParams.StartDate.Value);
        }

        if (filterParams.EndDate.HasValue)
        {
            whereConditions.Add("p.PaymentDate <= @EndDate");
            parameters.Add("EndDate", filterParams.EndDate.Value);
        }

        if (filterParams.MinAmount.HasValue)
        {
            whereConditions.Add("p.Amount >= @MinAmount");
            parameters.Add("MinAmount", filterParams.MinAmount.Value);
        }

        if (filterParams.MaxAmount.HasValue)
        {
            whereConditions.Add("p.Amount <= @MaxAmount");
            parameters.Add("MaxAmount", filterParams.MaxAmount.Value);
        }

        if (filterParams.UserId.HasValue)
        {
            whereConditions.Add("o.UserId = @UserId");
            parameters.Add("UserId", filterParams.UserId.Value);
        }

        if (filterParams.LocationId.HasValue)
        {
            whereConditions.Add("o.LocationId = @LocationId");
            parameters.Add("LocationId", filterParams.LocationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.CustomerName))
        {
            whereConditions.Add("o.CustomerName LIKE @CustomerName");
            parameters.Add("CustomerName", $"%{filterParams.CustomerName.Trim()}%");
        }

        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";
        return (whereClause, parameters);
    }

    private static string BuildOrderByClause(string? sortBy, string? sortDirection)
    {
        var validSortFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "id", "p.Id" },
            { "paymentdate", "p.PaymentDate" },
            { "amount", "p.Amount" },
            { "paymentmethod", "p.PaymentMethod" },
            { "status", "p.Status" },
            { "ordernumber", "o.OrderNumber" },
            { "orderdate", "o.OrderDate" },
            { "ordertotal", "o.Total" },
            { "customername", "o.CustomerName" },
            { "username", "u.Username" },
            { "locationname", "l.Name" },
            { "createdat", "p.CreatedAt" },
            { "updatedat", "p.UpdatedAt" }
        };

        var sortField = validSortFields.ContainsKey(sortBy ?? "paymentdate")
            ? validSortFields[sortBy ?? "paymentdate"]
            : validSortFields["paymentdate"];

        var direction = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";

        return $"ORDER BY {sortField} {direction}";
    }

    private static (int Offset, int PageSize) CalculatePagination(int pageNumber, int pageSize)
    {
        var offset = (pageNumber - 1) * pageSize;
        return (offset, pageSize);
    }

    private static bool ShouldPaginate(PaymentFilterParameters filterParams, int totalCount)
    {
        // Use pagination when:
        // 1. Total count is large (>1000 records)
        // 2. Specific page size is requested and reasonable
        // 3. Page number is greater than 1
        return totalCount > 1000 ||
               (filterParams.PageSize <= 100 && filterParams.PageNumber > 1) ||
               (filterParams.PageSize <= 50 && totalCount > 200);
    }

    private string BuildCountQuery(string whereClause)
    {
        return $@"
        SELECT COUNT(*)
        FROM {FullTableName} p
        INNER JOIN market.[Order] o ON p.OrderId = o.Id
        INNER JOIN auth.[User] u ON o.UserId = u.Id
        INNER JOIN market.Location l ON o.LocationId = l.Id
        {whereClause}";
    }

    private string BuildDataQuery(string whereClause, string orderByClause)
    {
        return $@"
        SELECT p.*,
               o.Id as OrderId_Split, o.OrderNumber, o.OrderDate, o.Total, o.CustomerName, o.CustomerPhone,
               u.Id as UserId_Split, u.Username, u.FirstName, u.LastName,
               l.Id as LocationId_Split, l.Name as LocationName, l.City
        FROM {FullTableName} p
        INNER JOIN market.[Order] o ON p.OrderId = o.Id
        INNER JOIN auth.[User] u ON o.UserId = u.Id
        INNER JOIN market.Location l ON o.LocationId = l.Id
        {whereClause}
        {orderByClause}";
    }

    private string BuildDataQueryWithPagination(string whereClause, string orderByClause)
    {
        return $@"
        SELECT p.*,
               o.Id as OrderId_Split, o.OrderNumber, o.OrderDate, o.Total, o.CustomerName, o.CustomerPhone,
               u.Id as UserId_Split, u.Username, u.FirstName, u.LastName,
               l.Id as LocationId_Split, l.Name as LocationName, l.City
        FROM {FullTableName} p
        INNER JOIN market.[Order] o ON p.OrderId = o.Id
        INNER JOIN auth.[User] u ON o.UserId = u.Id
        INNER JOIN market.Location l ON o.LocationId = l.Id
        {whereClause}
        {orderByClause}
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";
    }

    private static (int TotalPages, bool HasNextPage, bool HasPreviousPage) CalculatePaginationMetadata(
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var hasNextPage = pageNumber < totalPages;
        var hasPreviousPage = pageNumber > 1;

        return (totalPages, hasNextPage, hasPreviousPage);
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
            [PaymentStatus.Pending] = [PaymentStatus.Completed, PaymentStatus.Failed],
            [PaymentStatus.Completed] = [PaymentStatus.Refunded],
            [PaymentStatus.Failed] = [PaymentStatus.Pending], // Allow retry
            [PaymentStatus.Failed] = [], // No transitions from failed
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