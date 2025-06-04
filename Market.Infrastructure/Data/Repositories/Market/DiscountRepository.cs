using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Market;
using Market.Domain.Filters;
using Market.Infrastructure.Constants;

namespace Market.Infrastructure.Data.Repositories.Market;

public class DiscountRepository(IDbConnectionFactory connectionFactory) :
    GenericRepository<Discount>(connectionFactory, DatabaseConstants.Tables.Market.Discount, DatabaseConstants.Schemas.Market),
    IDiscountRepository
{
    public async Task<Discount?> GetByCodeAsync(string code)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE DiscountCode = @Code";
        var discount = await connection.QueryFirstOrDefaultAsync<Discount>(sql, new { Code = code });

        return discount ?? throw new KeyNotFoundException($"Discount with code '{code}' was not found.");
    }

    public async Task<IEnumerable<Discount>> GetActiveDiscountsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE IsActive = 1";
        return await connection.QueryAsync<Discount>(sql);
    }

    public async Task<IEnumerable<Discount>> GetDiscountsByLocationAsync(long locationId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE LocationId = @LocationId";
        return await connection.QueryAsync<Discount>(sql, new { LocationId = locationId });
    }

    public async Task<IEnumerable<Discount>> GetDiscountsByVendorAsync(long vendorId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE VendorId = @VendorId";
        return await connection.QueryAsync<Discount>(sql, new { VendorId = vendorId });
    }

    public async Task<IEnumerable<Discount>> GetValidDiscountsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                SELECT * FROM {FullTableName} 
                WHERE IsActive = 1 
                  AND (StartDate IS NULL OR StartDate <= GETUTCDATE()) 
                  AND (EndDate IS NULL OR EndDate >= GETUTCDATE())";
        return await connection.QueryAsync<Discount>(sql);
    }

    public async Task<bool> IsCodeExistsAsync(string code)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT COUNT(1) FROM {FullTableName} WHERE DiscountCode = @Code";
        var count = await connection.QuerySingleAsync<int>(sql, new { Code = code });
        return count > 0;
    }

    public async Task<PagedResult<Discount>> GetDiscountsAsync(DiscountFilterParameters filterParams)
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
            var discounts = await connection.QueryAsync<Discount, Location, Vendor, int, Discount>(
                dataSql,
                (discount, location, vendor, usageCount) =>
                {
                    discount.Location = location;
                    discount.Vendor = vendor;
                    // UsageCount can be used for additional info if needed
                    return discount;
                },
                parameters,
                splitOn: "LocationId_Split,VendorId_Split,UsageCount"
            );

            var paginationMetadata = CalculatePaginationMetadata(totalCount, filterParams.PageNumber, filterParams.PageSize);

            return new PagedResult<Discount>
            {
                Items = discounts.ToList(),
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
            var discounts = await connection.QueryAsync<Discount, Location, Vendor, Discount>(
                dataSql,
                (discount, location, vendor) =>
                {
                    discount.Location = location;
                    discount.Vendor = vendor;
                    return discount;
                },
                parameters,
                splitOn: "LocationId_Split,VendorId_Split"
            );

            var discountsList = discounts.ToList();

            return new PagedResult<Discount>
            {
                Items = discountsList,
                TotalCount = totalCount,
                Page = 1,
                PageSize = discountsList.Count,
                TotalPages = 1,
                HasNextPage = false,
                HasPreviousPage = false
            };
        }
    }

    public override async Task UpdateAsync(Discount entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var codeCheckSql = $"SELECT COUNT(1) FROM {FullTableName} WHERE DiscountCode = @DiscountCode AND Id != @Id";
        var codeExists = await connection.QuerySingleAsync<int>(codeCheckSql, new { DiscountCode = entity.DiscountCode, Id = entity.Id });
        if (codeExists > 0)
        {
            throw new ArgumentException($"Discount code '{entity.DiscountCode}' is already taken by another discount.");
        }

        if (entity.Percentage <= 0 || entity.Percentage > 100)
        {
            throw new ArgumentException("Discount percentage must be between 1 and 100.");
        }

        if (entity.StartDate.HasValue && entity.EndDate.HasValue && entity.StartDate > entity.EndDate)
        {
            throw new ArgumentException("Start date cannot be later than end date.");
        }

        if (entity.EndDate.HasValue && entity.EndDate < DateTime.UtcNow)
        {
            throw new ArgumentException("End date cannot be in the past.");
        }

        await base.UpdateAsync(entity);
    }

    public override async Task<Discount> AddAsync(Discount entity)
    {
        if (await IsCodeExistsAsync(entity.DiscountCode))
        {
            throw new ArgumentException($"Discount code '{entity.DiscountCode}' is already taken.");
        }

        if (entity.Percentage <= 0 || entity.Percentage > 100)
        {
            throw new ArgumentException("Discount percentage must be between 1 and 100.");
        }

        if (entity.StartDate.HasValue && entity.EndDate.HasValue && entity.StartDate > entity.EndDate)
        {
            throw new ArgumentException("Start date cannot be later than end date.");
        }

        if (entity.EndDate.HasValue && entity.EndDate < DateTime.UtcNow)
        {
            throw new ArgumentException("End date cannot be in the past.");
        }

        if (!entity.LocationId.HasValue && !entity.VendorId.HasValue)
        {
            throw new ArgumentException("Discount must be associated with either a location or a vendor.");
        }

        if (entity.LocationId.HasValue && entity.VendorId.HasValue)
        {
            throw new ArgumentException("Discount cannot be associated with both a location and a vendor simultaneously.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();

        if (entity.LocationId.HasValue)
        {
            var locationCheckSql = "SELECT COUNT(1) FROM market.Location WHERE Id = @LocationId";
            var locationExists = await connection.QuerySingleAsync<int>(locationCheckSql, new { LocationId = entity.LocationId });
            if (locationExists == 0)
            {
                throw new ArgumentException($"Location with ID '{entity.LocationId}' does not exist.");
            }
        }

        if (entity.VendorId.HasValue)
        {
            var vendorCheckSql = "SELECT COUNT(1) FROM market.Vendor WHERE Id = @VendorId";
            var vendorExists = await connection.QuerySingleAsync<int>(vendorCheckSql, new { VendorId = entity.VendorId });
            if (vendorExists == 0)
            {
                throw new ArgumentException($"Vendor with ID '{entity.VendorId}' does not exist.");
            }
        }

        return await base.AddAsync(entity);
    }

    public override async Task DeleteAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var discountCheckSql = $@"
            SELECT COUNT(1) FROM {FullTableName} 
            WHERE Id = @Id 
              AND IsActive = 1 
              AND (StartDate IS NULL OR StartDate <= GETUTCDATE()) 
              AND (EndDate IS NULL OR EndDate >= GETUTCDATE())";

        var isActiveAndValid = await connection.QuerySingleAsync<int>(discountCheckSql, new { Id = id });
        if (isActiveAndValid > 0)
        {
            throw new InvalidOperationException($"Cannot delete discount with ID '{id}' because it is currently active and valid. Deactivate it first or wait until it expires.");
        }

        await base.DeleteAsync(id);
    }

    private static (string WhereClause, DynamicParameters Parameters) BuildWhereClause(DiscountFilterParameters filterParams)
    {
        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
        {
            whereConditions.Add("(d.DiscountCode LIKE @SearchTerm OR d.Description LIKE @SearchTerm OR l.Name LIKE @SearchTerm OR v.Name LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{filterParams.SearchTerm.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filterParams.DiscountCode))
        {
            whereConditions.Add("d.DiscountCode LIKE @DiscountCode");
            parameters.Add("DiscountCode", $"%{filterParams.DiscountCode.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Description))
        {
            whereConditions.Add("d.Description LIKE @Description");
            parameters.Add("Description", $"%{filterParams.Description.Trim()}%");
        }

        if (filterParams.IsActive.HasValue)
        {
            whereConditions.Add("d.IsActive = @IsActive");
            parameters.Add("IsActive", filterParams.IsActive.Value);
        }

        if (filterParams.IsValid.HasValue && filterParams.IsValid.Value)
        {
            whereConditions.Add("d.IsActive = 1 AND (d.StartDate IS NULL OR d.StartDate <= GETUTCDATE()) AND (d.EndDate IS NULL OR d.EndDate >= GETUTCDATE())");
        }
        else if (filterParams.IsValid.HasValue && !filterParams.IsValid.Value)
        {
            whereConditions.Add("(d.IsActive = 0 OR (d.StartDate IS NOT NULL AND d.StartDate > GETUTCDATE()) OR (d.EndDate IS NOT NULL AND d.EndDate < GETUTCDATE()))");
        }

        if (filterParams.MinPercentage.HasValue)
        {
            whereConditions.Add("d.Percentage >= @MinPercentage");
            parameters.Add("MinPercentage", filterParams.MinPercentage.Value);
        }

        if (filterParams.MaxPercentage.HasValue)
        {
            whereConditions.Add("d.Percentage <= @MaxPercentage");
            parameters.Add("MaxPercentage", filterParams.MaxPercentage.Value);
        }

        if (filterParams.LocationId.HasValue)
        {
            whereConditions.Add("d.LocationId = @LocationId");
            parameters.Add("LocationId", filterParams.LocationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.LocationName))
        {
            whereConditions.Add("l.Name LIKE @LocationName");
            parameters.Add("LocationName", $"%{filterParams.LocationName.Trim()}%");
        }

        if (filterParams.VendorId.HasValue)
        {
            whereConditions.Add("d.VendorId = @VendorId");
            parameters.Add("VendorId", filterParams.VendorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.VendorName))
        {
            whereConditions.Add("v.Name LIKE @VendorName");
            parameters.Add("VendorName", $"%{filterParams.VendorName.Trim()}%");
        }

        if (filterParams.StartDateFrom.HasValue)
        {
            whereConditions.Add("(d.StartDate IS NULL OR d.StartDate >= @StartDateFrom)");
            parameters.Add("StartDateFrom", filterParams.StartDateFrom.Value);
        }

        if (filterParams.StartDateTo.HasValue)
        {
            whereConditions.Add("(d.StartDate IS NULL OR d.StartDate <= @StartDateTo)");
            parameters.Add("StartDateTo", filterParams.StartDateTo.Value);
        }

        if (filterParams.EndDateFrom.HasValue)
        {
            whereConditions.Add("(d.EndDate IS NULL OR d.EndDate >= @EndDateFrom)");
            parameters.Add("EndDateFrom", filterParams.EndDateFrom.Value);
        }

        if (filterParams.EndDateTo.HasValue)
        {
            whereConditions.Add("(d.EndDate IS NULL OR d.EndDate <= @EndDateTo)");
            parameters.Add("EndDateTo", filterParams.EndDateTo.Value);
        }

        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";
        return (whereClause, parameters);
    }

    private static string BuildOrderByClause(string? sortBy, string? sortDirection)
    {
        var validSortFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "id", "d.Id" },
            { "discountcode", "d.DiscountCode" },
            { "description", "d.Description" },
            { "percentage", "d.Percentage" },
            { "startdate", "d.StartDate" },
            { "enddate", "d.EndDate" },
            { "isactive", "d.IsActive" },
            { "locationname", "l.Name" },
            { "vendorname", "v.Name" },
            { "createdat", "d.CreatedAt" },
            { "updatedat", "d.UpdatedAt" }
        };

        var sortField = validSortFields.ContainsKey(sortBy ?? "createdat")
            ? validSortFields[sortBy ?? "createdat"]
            : validSortFields["createdat"];

        var direction = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";

        return $"ORDER BY {sortField} {direction}";
    }

    private static (int Offset, int PageSize) CalculatePagination(int pageNumber, int pageSize)
    {
        var offset = (pageNumber - 1) * pageSize;
        return (offset, pageSize);
    }

    private static bool ShouldPaginate(DiscountFilterParameters filterParams, int totalCount)
    {
        return totalCount > 200 ||
               (filterParams.PageSize <= 100 && filterParams.PageNumber > 1) ||
               (filterParams.PageSize <= 50 && totalCount > 50);
    }

    private string BuildCountQuery(string whereClause)
    {
        return $@"
        SELECT COUNT(*)
        FROM {FullTableName} d
        LEFT JOIN market.Location l ON d.LocationId = l.Id
        LEFT JOIN market.Vendor v ON d.VendorId = v.Id
        {whereClause}";
    }

    private string BuildDataQuery(string whereClause, string orderByClause)
    {
        return $@"
        SELECT d.*,
               l.Id as LocationId_Split, l.Name as LocationName, l.City, l.Country,
               v.Id as VendorId_Split, v.Name as VendorName, v.ContactPersonName, v.Email as VendorEmail
        FROM {FullTableName} d
        LEFT JOIN market.Location l ON d.LocationId = l.Id
        LEFT JOIN market.Vendor v ON d.VendorId = v.Id
        {whereClause}
        {orderByClause}";
    }

    private string BuildDataQueryWithPagination(string whereClause, string orderByClause)
    {
        return $@"
        SELECT d.*,
               l.Id as LocationId_Split, l.Name as LocationName, l.City, l.Country,
               v.Id as VendorId_Split, v.Name as VendorName, v.ContactPersonName, v.Email as VendorEmail,
               ISNULL(o.UsageCount, 0) as UsageCount
        FROM {FullTableName} d
        LEFT JOIN market.Location l ON d.LocationId = l.Id
        LEFT JOIN market.Vendor v ON d.VendorId = v.Id
        LEFT JOIN (
            SELECT DiscountId, COUNT(*) as UsageCount 
            FROM market.[Order] 
            WHERE DiscountId IS NOT NULL
            GROUP BY DiscountId
        ) o ON d.Id = o.DiscountId
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

    protected override string GenerateInsertQuery()
    {
        return @"
                INSERT INTO market.Discount (DiscountCode, Description, Percentage, StartDate, EndDate, IsActive, LocationId, VendorId, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
                VALUES (@DiscountCode, @Description, @Percentage, @StartDate, @EndDate, @IsActive, @LocationId, @VendorId, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
    }

    protected override string GenerateUpdateQuery()
    {
        return @"
                UPDATE market.Discount 
                SET DiscountCode = @DiscountCode, 
                    Description = @Description, 
                    Percentage = @Percentage, 
                    StartDate = @StartDate, 
                    EndDate = @EndDate, 
                    IsActive = @IsActive, 
                    LocationId = @LocationId, 
                    VendorId = @VendorId, 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
    }
}