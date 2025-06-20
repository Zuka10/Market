﻿using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Auth;
using Market.Domain.Entities.Market;
using Market.Domain.Enums;
using Market.Domain.Filters;
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
        var order = await connection.QueryFirstOrDefaultAsync<Order>(sql, new { OrderNumber = orderNumber });

        return order ?? throw new KeyNotFoundException($"Order with order number '{orderNumber}' was not found.");
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

        var result = orderDict.Values.FirstOrDefault();
        return result ?? throw new KeyNotFoundException($"Order with ID '{id}' was not found.");
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

    public async Task<decimal> GetTotalSalesByLocationAsync(long locationId, DateTime? startDate = null, DateTime? endDate = null)
    {
        using var locationConnection = await _connectionFactory.CreateConnectionAsync();
        var locationCheckSql = "SELECT COUNT(1) FROM market.Location WHERE Id = @LocationId";
        var locationExists = await locationConnection.QuerySingleAsync<int>(locationCheckSql, new { LocationId = locationId });
        if (locationExists == 0)
        {
            throw new KeyNotFoundException($"Location with ID '{locationId}' was not found.");
        }

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

    public async Task<PagedResult<Order>> GetOrdersAsync(OrderFilterParameters filterParams)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var (whereClause, parameters) = BuildWhereClause(filterParams);
        var orderByClause = BuildOrderByClause(filterParams.SortBy, filterParams.SortDirection);
        var (offset, pageSize) = CalculatePagination(filterParams.PageNumber, filterParams.PageSize);

        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        var countSql = BuildCountQuery(whereClause);
        var dataSql = BuildDataQueryWithDetails(whereClause, orderByClause);

        // Execute queries
        var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);
        var orders = await connection.QueryAsync<Order, Location, User, int, Order>(
            dataSql,
            (order, location, user, orderDetailCount) =>
            {
                order.Location = location;
                order.User = user;
                return order;
            },
            parameters,
            splitOn: "LocationId_Split,UserId_Split,OrderDetailCount"
        );

        var paginationMetadata = CalculatePaginationMetadata(totalCount, filterParams.PageNumber, filterParams.PageSize);

        return new PagedResult<Order>
        {
            Items = orders.ToList(),
            TotalCount = totalCount,
            Page = filterParams.PageNumber,
            PageSize = filterParams.PageSize,
            TotalPages = paginationMetadata.TotalPages,
            HasNextPage = paginationMetadata.HasNextPage,
            HasPreviousPage = paginationMetadata.HasPreviousPage
        };
    }

    public override async Task UpdateAsync(Order entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var orderNumberCheckSql = $"SELECT COUNT(1) FROM {FullTableName} WHERE OrderNumber = @OrderNumber AND Id != @Id";
        var orderNumberExists = await connection.QuerySingleAsync<int>(orderNumberCheckSql, new { OrderNumber = entity.OrderNumber, Id = entity.Id });
        if (orderNumberExists > 0)
        {
            throw new ArgumentException($"Order number '{entity.OrderNumber}' is already taken by another order.");
        }

        await ValidateForeignKeys(entity);

        ValidateOrderAmounts(entity);

        await ValidateOrderStatusTransition(entity);

        await base.UpdateAsync(entity);
    }

    public override async Task<Order> AddAsync(Order entity)
    {
        if (await IsOrderNumberExistsAsync(entity.OrderNumber))
        {
            throw new ArgumentException($"Order number '{entity.OrderNumber}' is already taken.");
        }

        await ValidateForeignKeys(entity);

        ValidateOrderAmounts(entity);

        if (entity.OrderDate > DateTime.UtcNow.AddDays(1))
        {
            throw new ArgumentException("Order date cannot be more than 1 day in the future.");
        }

        return await base.AddAsync(entity);
    }

    public override async Task DeleteAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var statusCheckSql = $"SELECT Status FROM {FullTableName} WHERE Id = @Id";
        var status = await connection.QuerySingleAsync<int>(statusCheckSql, new { Id = id });

        if (status != (int)OrderStatus.Pending && status != (int)OrderStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot delete order with ID '{id}' because it has status '{(OrderStatus)status}'. Only Pending or Cancelled orders can be deleted.");
        }

        var paymentCheckSql = "SELECT COUNT(1) FROM market.Payment WHERE OrderId = @OrderId";
        var hasPayments = await connection.QuerySingleAsync<int>(paymentCheckSql, new { OrderId = id });
        if (hasPayments > 0)
        {
            throw new InvalidOperationException($"Cannot delete order with ID '{id}' because it has associated payments. Cancel the order instead.");
        }

        await base.DeleteAsync(id);
    }

    private static (string WhereClause, DynamicParameters Parameters) BuildWhereClause(OrderFilterParameters filterParams)
    {
        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
        {
            whereConditions.Add("(o.OrderNumber LIKE @SearchTerm OR o.CustomerName LIKE @SearchTerm OR o.CustomerPhone LIKE @SearchTerm OR u.Username LIKE @SearchTerm OR u.FirstName LIKE @SearchTerm OR u.LastName LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{filterParams.SearchTerm.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filterParams.OrderNumber))
        {
            whereConditions.Add("o.OrderNumber LIKE @OrderNumber");
            parameters.Add("OrderNumber", $"%{filterParams.OrderNumber.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filterParams.CustomerName))
        {
            whereConditions.Add("o.CustomerName LIKE @CustomerName");
            parameters.Add("CustomerName", $"%{filterParams.CustomerName.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filterParams.CustomerPhone))
        {
            whereConditions.Add("o.CustomerPhone LIKE @CustomerPhone");
            parameters.Add("CustomerPhone", $"%{filterParams.CustomerPhone.Trim()}%");
        }

        if (filterParams.Status.HasValue)
        {
            whereConditions.Add("o.Status = @Status");
            parameters.Add("Status", (int)filterParams.Status.Value);
        }

        if (filterParams.LocationId.HasValue)
        {
            whereConditions.Add("o.LocationId = @LocationId");
            parameters.Add("LocationId", filterParams.LocationId.Value);
        }

        if (filterParams.UserId.HasValue)
        {
            whereConditions.Add("o.UserId = @UserId");
            parameters.Add("UserId", filterParams.UserId.Value);
        }

        if (filterParams.DiscountId.HasValue)
        {
            whereConditions.Add("o.DiscountId = @DiscountId");
            parameters.Add("DiscountId", filterParams.DiscountId.Value);
        }

        if (filterParams.StartDate.HasValue)
        {
            whereConditions.Add("o.OrderDate >= @StartDate");
            parameters.Add("StartDate", filterParams.StartDate.Value);
        }

        if (filterParams.EndDate.HasValue)
        {
            whereConditions.Add("o.OrderDate <= @EndDate");
            parameters.Add("EndDate", filterParams.EndDate.Value);
        }

        if (filterParams.MinTotal.HasValue)
        {
            whereConditions.Add("o.Total >= @MinTotal");
            parameters.Add("MinTotal", filterParams.MinTotal.Value);
        }

        if (filterParams.MaxTotal.HasValue)
        {
            whereConditions.Add("o.Total <= @MaxTotal");
            parameters.Add("MaxTotal", filterParams.MaxTotal.Value);
        }

        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";
        return (whereClause, parameters);
    }

    private static string BuildOrderByClause(string? sortBy, string? sortDirection)
    {
        var validSortFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "id", "o.Id" },
            { "ordernumber", "o.OrderNumber" },
            { "orderdate", "o.OrderDate" },
            { "total", "o.Total" },
            { "subtotal", "o.SubTotal" },
            { "status", "o.Status" },
            { "customername", "o.CustomerName" },
            { "customerphone", "o.CustomerPhone" },
            { "locationname", "l.Name" },
            { "username", "u.Username" },
            { "createdat", "o.CreatedAt" },
            { "updatedat", "o.UpdatedAt" }
        };

        var sortField = validSortFields.ContainsKey(sortBy ?? "orderdate")
            ? validSortFields[sortBy ?? "orderdate"]
            : validSortFields["orderdate"];

        var direction = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";

        return $"ORDER BY {sortField} {direction}";
    }

    private static (int Offset, int PageSize) CalculatePagination(int pageNumber, int pageSize)
    {
        var offset = (pageNumber - 1) * pageSize;
        return (offset, pageSize);
    }

    private string BuildCountQuery(string whereClause)
    {
        return $@"
        SELECT COUNT(*)
        FROM {FullTableName} o
        INNER JOIN market.Location l ON o.LocationId = l.Id
        INNER JOIN auth.[User] u ON o.UserId = u.Id
        {whereClause}";
    }

    private string BuildDataQueryWithDetails(string whereClause, string orderByClause)
    {
        return $@"
        SELECT o.*,
               l.Id as LocationId_Split, l.Name as LocationName, l.City,
               u.Id as UserId_Split, u.Username, u.FirstName, u.LastName,
               ISNULL(od.OrderDetailCount, 0) as OrderDetailCount
        FROM {FullTableName} o
        INNER JOIN market.Location l ON o.LocationId = l.Id
        INNER JOIN auth.[User] u ON o.UserId = u.Id
        LEFT JOIN (
            SELECT OrderId, COUNT(*) as OrderDetailCount 
            FROM market.OrderDetail 
            GROUP BY OrderId
        ) od ON o.Id = od.OrderId
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
    private async Task ValidateForeignKeys(Order entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        // Validate LocationId
        var locationCheckSql = "SELECT COUNT(1) FROM market.Location WHERE Id = @LocationId AND IsActive = 1";
        var locationExists = await connection.QuerySingleAsync<int>(locationCheckSql, new { entity.LocationId });
        if (locationExists == 0)
        {
            throw new ArgumentException($"Location with ID '{entity.LocationId}' does not exist or is not active.");
        }

        // Validate UserId
        var userCheckSql = "SELECT COUNT(1) FROM auth.[User] WHERE Id = @UserId AND IsActive = 1";
        var userExists = await connection.QuerySingleAsync<int>(userCheckSql, new { entity.UserId });
        if (userExists == 0)
        {
            throw new ArgumentException($"User with ID '{entity.UserId}' does not exist or is not active.");
        }

        // Validate DiscountId if provided
        if (entity.DiscountId.HasValue)
        {
            var discountCheckSql = @"
                SELECT COUNT(1) FROM market.Discount 
                WHERE Id = @DiscountId 
                  AND IsActive = 1 
                  AND (StartDate IS NULL OR StartDate <= GETUTCDATE()) 
                  AND (EndDate IS NULL OR EndDate >= GETUTCDATE())";
            var discountExists = await connection.QuerySingleAsync<int>(discountCheckSql, new { entity.DiscountId });
            if (discountExists == 0)
            {
                throw new ArgumentException($"Discount with ID '{entity.DiscountId}' does not exist or is not currently valid.");
            }
        }
    }

    private static void ValidateOrderAmounts(Order entity)
    {
        if (entity.Total < 0)
        {
            throw new ArgumentException("Order total cannot be negative.");
        }

        if (entity.SubTotal < 0)
        {
            throw new ArgumentException("Order subtotal cannot be negative.");
        }

        if (entity.TotalCommission < 0)
        {
            throw new ArgumentException("Order commission cannot be negative.");
        }

        if (entity.DiscountAmount < 0)
        {
            throw new ArgumentException("Discount amount cannot be negative.");
        }

        if (entity.DiscountAmount > entity.SubTotal)
        {
            throw new ArgumentException("Discount amount cannot exceed subtotal.");
        }
    }

    private async Task ValidateOrderStatusTransition(Order entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var currentStatusSql = $"SELECT Status FROM {FullTableName} WHERE Id = @Id";
        var currentStatus = await connection.QuerySingleAsync<int>(currentStatusSql, new { Id = entity.Id });

        var currentOrderStatus = (OrderStatus)currentStatus;
        var newOrderStatus = entity.Status;

        // Define valid status transitions
        var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
        {
            [OrderStatus.Pending] = [OrderStatus.Completed, OrderStatus.Cancelled],
            [OrderStatus.Completed] = [OrderStatus.Pending, OrderStatus.Cancelled],
            [OrderStatus.Pending] = [OrderStatus.Completed, OrderStatus.Cancelled],
            [OrderStatus.Completed] = [], // No transitions from completed
            [OrderStatus.Cancelled] = [] // No transitions from cancelled
        };

        if (!validTransitions[currentOrderStatus].Contains(newOrderStatus))
        {
            throw new InvalidOperationException($"Invalid status transition from '{currentOrderStatus}' to '{newOrderStatus}'.");
        }
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