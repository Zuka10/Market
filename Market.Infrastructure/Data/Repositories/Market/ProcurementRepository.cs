using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Market;
using Market.Domain.Filters;
using Market.Infrastructure.Constants;

namespace Market.Infrastructure.Data.Repositories.Market;

public class ProcurementRepository(IDbConnectionFactory connectionFactory) :
    GenericRepository<Procurement>(connectionFactory, DatabaseConstants.Tables.Market.Procurement, DatabaseConstants.Schemas.Market),
    IProcurementRepository
{
    public async Task<Procurement?> GetByReferenceNoAsync(string referenceNo)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE ReferenceNo = @ReferenceNo";
        var procurement = await connection.QueryFirstOrDefaultAsync<Procurement>(sql, new { ReferenceNo = referenceNo });

        return procurement ?? throw new KeyNotFoundException($"Procurement with reference number '{referenceNo}' was not found.");
    }

    public async Task<Procurement?> GetProcurementWithDetailsAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT p.*, pd.Id, pd.ProcurementId, pd.ProductId, pd.PurchasePrice, pd.Quantity, pd.LineTotal,
                       pr.Id, pr.Name, pr.Description, pr.Unit,
                       v.Id, v.Name as VendorName, v.ContactPersonName,
                       l.Id, l.Name as LocationName, l.City
                FROM market.Procurement p
                LEFT JOIN market.ProcurementDetail pd ON p.Id = pd.ProcurementId
                LEFT JOIN market.Product pr ON pd.ProductId = pr.Id
                INNER JOIN market.Vendor v ON p.VendorId = v.Id
                INNER JOIN market.Location l ON p.LocationId = l.Id
                WHERE p.Id = @Id";

        var procurementDict = new Dictionary<long, Procurement>();

        await connection.QueryAsync<Procurement, ProcurementDetail, Product, Vendor, Location, Procurement>(
            sql,
            (procurement, procurementDetail, product, vendor, location) =>
            {
                if (!procurementDict.TryGetValue(procurement.Id, out var existingProcurement))
                {
                    existingProcurement = procurement;
                    existingProcurement.ProcurementDetails = new List<ProcurementDetail>();
                    existingProcurement.Vendor = vendor;
                    existingProcurement.Location = location;
                    procurementDict.Add(procurement.Id, existingProcurement);
                }

                if (procurementDetail != null)
                {
                    procurementDetail.Product = product;
                    existingProcurement.ProcurementDetails.Add(procurementDetail);
                }

                return existingProcurement;
            },
            new { Id = id },
            splitOn: "Id,Id,Id,Id");

        var result = procurementDict.Values.FirstOrDefault();
        return result ?? throw new KeyNotFoundException($"Procurement with ID '{id}' was not found.");
    }

    public async Task<IEnumerable<Procurement>> GetProcurementsWithDetailsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT p.*, v.Id, v.Name as VendorName, v.ContactPersonName,
                       l.Id, l.Name as LocationName, l.City
                FROM market.Procurement p
                INNER JOIN market.Vendor v ON p.VendorId = v.Id
                INNER JOIN market.Location l ON p.LocationId = l.Id
                ORDER BY p.ProcurementDate DESC";

        var result = await connection.QueryAsync<Procurement, Vendor, Location, Procurement>(
            sql,
            (procurement, vendor, location) =>
            {
                procurement.Vendor = vendor;
                procurement.Location = location;
                return procurement;
            },
            splitOn: "Id,Id");

        return result;
    }

    public async Task<IEnumerable<Procurement>> GetProcurementsByVendorAsync(long vendorId)
    {
        using var vendorConnection = await _connectionFactory.CreateConnectionAsync();
        var vendorCheckSql = "SELECT COUNT(1) FROM market.Vendor WHERE Id = @VendorId";
        var vendorExists = await vendorConnection.QuerySingleAsync<int>(vendorCheckSql, new { VendorId = vendorId });
        if (vendorExists == 0)
        {
            throw new KeyNotFoundException($"Vendor with ID '{vendorId}' was not found.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE VendorId = @VendorId ORDER BY ProcurementDate DESC";
        return await connection.QueryAsync<Procurement>(sql, new { VendorId = vendorId });
    }

    public async Task<IEnumerable<Procurement>> GetProcurementsByLocationAsync(long locationId)
    {
        using var locationConnection = await _connectionFactory.CreateConnectionAsync();
        var locationCheckSql = "SELECT COUNT(1) FROM market.Location WHERE Id = @LocationId";
        var locationExists = await locationConnection.QuerySingleAsync<int>(locationCheckSql, new { LocationId = locationId });
        if (locationExists == 0)
        {
            throw new KeyNotFoundException($"Location with ID '{locationId}' was not found.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE LocationId = @LocationId ORDER BY ProcurementDate DESC";
        return await connection.QueryAsync<Procurement>(sql, new { LocationId = locationId });
    }

    public async Task<IEnumerable<Procurement>> GetProcurementsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                SELECT * FROM {FullTableName} 
                WHERE ProcurementDate >= @StartDate AND ProcurementDate <= @EndDate 
                ORDER BY ProcurementDate DESC";
        return await connection.QueryAsync<Procurement>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<decimal> GetTotalProcurementValueAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var whereClause = "WHERE 1=1";

        if (startDate.HasValue)
        {
            whereClause += " AND ProcurementDate >= @StartDate";
        }

        if (endDate.HasValue)
        {
            whereClause += " AND ProcurementDate <= @EndDate";
        }

        var sql = $"SELECT ISNULL(SUM(TotalAmount), 0) FROM {FullTableName} {whereClause}";
        return await connection.QuerySingleAsync<decimal>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<bool> IsReferenceNoExistsAsync(string referenceNo)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT COUNT(1) FROM {FullTableName} WHERE ReferenceNo = @ReferenceNo";
        var count = await connection.QuerySingleAsync<int>(sql, new { ReferenceNo = referenceNo });
        return count > 0;
    }

    public async Task<PagedResult<Procurement>> GetProcurementsAsync(ProcurementFilterParameters filterParams)
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
            var procurements = await connection.QueryAsync<Procurement, Vendor, Location, int, Procurement>(
                dataSql,
                (procurement, vendor, location, detailCount) =>
                {
                    procurement.Vendor = vendor;
                    procurement.Location = location;
                    // DetailCount can be used for additional info if needed
                    return procurement;
                },
                parameters,
                splitOn: "VendorId_Split,LocationId_Split,DetailCount"
            );

            var paginationMetadata = CalculatePaginationMetadata(totalCount, filterParams.PageNumber, filterParams.PageSize);

            return new PagedResult<Procurement>
            {
                Items = procurements.ToList(),
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
            var procurements = await connection.QueryAsync<Procurement, Vendor, Location, Procurement>(
                dataSql,
                (procurement, vendor, location) =>
                {
                    procurement.Vendor = vendor;
                    procurement.Location = location;
                    return procurement;
                },
                parameters,
                splitOn: "VendorId_Split,LocationId_Split"
            );

            var procurementsList = procurements.ToList();

            return new PagedResult<Procurement>
            {
                Items = procurementsList,
                TotalCount = totalCount,
                Page = 1,
                PageSize = procurementsList.Count,
                TotalPages = 1,
                HasNextPage = false,
                HasPreviousPage = false
            };
        }
    }

    public override async Task UpdateAsync(Procurement entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var referenceCheckSql = $"SELECT COUNT(1) FROM {FullTableName} WHERE ReferenceNo = @ReferenceNo AND Id != @Id";
        var referenceExists = await connection.QuerySingleAsync<int>(referenceCheckSql, new { ReferenceNo = entity.ReferenceNo, Id = entity.Id });
        if (referenceExists > 0)
        {
            throw new ArgumentException($"Procurement reference number '{entity.ReferenceNo}' is already taken by another procurement.");
        }

        await ValidateForeignKeys(entity);

        ValidateProcurementRules(entity);

        await base.UpdateAsync(entity);
    }

    public override async Task<Procurement> AddAsync(Procurement entity)
    {
        if (await IsReferenceNoExistsAsync(entity.ReferenceNo!))
        {
            throw new ArgumentException($"Procurement reference number '{entity.ReferenceNo}' is already taken.");
        }

        await ValidateForeignKeys(entity);

        ValidateProcurementRules(entity);

        if (entity.ProcurementDate > DateTime.UtcNow.AddDays(1))
        {
            throw new ArgumentException("Procurement date cannot be more than 1 day in the future.");
        }

        return entity.ProcurementDate < DateTime.UtcNow.AddYears(-5)
            ? throw new ArgumentException("Procurement date cannot be more than 5 years in the past.")
            : await base.AddAsync(entity);
    }

    public override async Task DeleteAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var detailsCheckSql = "SELECT COUNT(1) FROM market.ProcurementDetail WHERE ProcurementId = @ProcurementId";
        var hasDetails = await connection.QuerySingleAsync<int>(detailsCheckSql, new { ProcurementId = id });
        if (hasDetails > 0)
        {
            throw new InvalidOperationException($"Cannot delete procurement with ID '{id}' because it has associated procurement details. Remove all details first.");
        }

        await base.DeleteAsync(id);
    }
    private static (string WhereClause, DynamicParameters Parameters) BuildWhereClause(ProcurementFilterParameters filterParams)
    {
        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
        {
            whereConditions.Add("(p.ReferenceNo LIKE @SearchTerm OR v.Name LIKE @SearchTerm OR v.ContactPersonName LIKE @SearchTerm OR l.Name LIKE @SearchTerm OR p.Notes LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{filterParams.SearchTerm.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filterParams.ReferenceNo))
        {
            whereConditions.Add("p.ReferenceNo LIKE @ReferenceNo");
            parameters.Add("ReferenceNo", $"%{filterParams.ReferenceNo.Trim()}%");
        }

        if (filterParams.VendorId.HasValue)
        {
            whereConditions.Add("p.VendorId = @VendorId");
            parameters.Add("VendorId", filterParams.VendorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.VendorName))
        {
            whereConditions.Add("v.Name LIKE @VendorName");
            parameters.Add("VendorName", $"%{filterParams.VendorName.Trim()}%");
        }

        if (filterParams.LocationId.HasValue)
        {
            whereConditions.Add("p.LocationId = @LocationId");
            parameters.Add("LocationId", filterParams.LocationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.LocationName))
        {
            whereConditions.Add("l.Name LIKE @LocationName");
            parameters.Add("LocationName", $"%{filterParams.LocationName.Trim()}%");
        }

        if (filterParams.StartDate.HasValue)
        {
            whereConditions.Add("p.ProcurementDate >= @StartDate");
            parameters.Add("StartDate", filterParams.StartDate.Value);
        }

        if (filterParams.EndDate.HasValue)
        {
            whereConditions.Add("p.ProcurementDate <= @EndDate");
            parameters.Add("EndDate", filterParams.EndDate.Value);
        }

        if (filterParams.MinAmount.HasValue)
        {
            whereConditions.Add("p.TotalAmount >= @MinAmount");
            parameters.Add("MinAmount", filterParams.MinAmount.Value);
        }

        if (filterParams.MaxAmount.HasValue)
        {
            whereConditions.Add("p.TotalAmount <= @MaxAmount");
            parameters.Add("MaxAmount", filterParams.MaxAmount.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Notes))
        {
            whereConditions.Add("p.Notes LIKE @Notes");
            parameters.Add("Notes", $"%{filterParams.Notes.Trim()}%");
        }

        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";
        return (whereClause, parameters);
    }

    private static string BuildOrderByClause(string? sortBy, string? sortDirection)
    {
        var validSortFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "id", "p.Id" },
            { "referenceno", "p.ReferenceNo" },
            { "procurementdate", "p.ProcurementDate" },
            { "totalamount", "p.TotalAmount" },
            { "vendorname", "v.Name" },
            { "locationname", "l.Name" },
            { "notes", "p.Notes" },
            { "createdat", "p.CreatedAt" },
            { "updatedat", "p.UpdatedAt" }
        };

        var sortField = validSortFields.ContainsKey(sortBy ?? "procurementdate")
            ? validSortFields[sortBy ?? "procurementdate"]
            : validSortFields["procurementdate"];

        var direction = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";

        return $"ORDER BY {sortField} {direction}";
    }

    private static (int Offset, int PageSize) CalculatePagination(int pageNumber, int pageSize)
    {
        var offset = (pageNumber - 1) * pageSize;
        return (offset, pageSize);
    }

    private static bool ShouldPaginate(ProcurementFilterParameters filterParams, int totalCount)
    {
        // Use pagination when:
        // 1. Total count is large (>500 records for procurement data)
        // 2. Specific page size is requested and reasonable
        // 3. Page number is greater than 1
        return totalCount > 500 ||
               (filterParams.PageSize <= 100 && filterParams.PageNumber > 1) ||
               (filterParams.PageSize <= 50 && totalCount > 100);
    }

    private string BuildCountQuery(string whereClause)
    {
        return $@"
        SELECT COUNT(*)
        FROM {FullTableName} p
        INNER JOIN market.Vendor v ON p.VendorId = v.Id
        INNER JOIN market.Location l ON p.LocationId = l.Id
        {whereClause}";
    }

    private string BuildDataQuery(string whereClause, string orderByClause)
    {
        return $@"
        SELECT p.*,
               v.Id as VendorId_Split, v.Name as VendorName, v.ContactPersonName, v.Email as VendorEmail,
               l.Id as LocationId_Split, l.Name as LocationName, l.City, l.Country
        FROM {FullTableName} p
        INNER JOIN market.Vendor v ON p.VendorId = v.Id
        INNER JOIN market.Location l ON p.LocationId = l.Id
        {whereClause}
        {orderByClause}";
    }

    private string BuildDataQueryWithPagination(string whereClause, string orderByClause)
    {
        return $@"
        SELECT p.*,
               v.Id as VendorId_Split, v.Name as VendorName, v.ContactPersonName, v.Email as VendorEmail,
               l.Id as LocationId_Split, l.Name as LocationName, l.City, l.Country,
               ISNULL(pd.DetailCount, 0) as DetailCount
        FROM {FullTableName} p
        INNER JOIN market.Vendor v ON p.VendorId = v.Id
        INNER JOIN market.Location l ON p.LocationId = l.Id
        LEFT JOIN (
            SELECT ProcurementId, COUNT(*) as DetailCount 
            FROM market.ProcurementDetail 
            GROUP BY ProcurementId
        ) pd ON p.Id = pd.ProcurementId
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

    private async Task ValidateForeignKeys(Procurement entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var vendorCheckSql = "SELECT COUNT(1) FROM market.Vendor WHERE Id = @VendorId";
        var vendorExists = await connection.QuerySingleAsync<int>(vendorCheckSql, new { VendorId = entity.VendorId });
        if (vendorExists == 0)
        {
            throw new ArgumentException($"Vendor with ID '{entity.VendorId}' does not exist.");
        }

        var locationCheckSql = "SELECT COUNT(1) FROM market.Location WHERE Id = @LocationId AND IsActive = 1";
        var locationExists = await connection.QuerySingleAsync<int>(locationCheckSql, new { LocationId = entity.LocationId });
        if (locationExists == 0)
        {
            throw new ArgumentException($"Location with ID '{entity.LocationId}' does not exist or is not active.");
        }
    }

    private static void ValidateProcurementRules(Procurement entity)
    {
        if (string.IsNullOrWhiteSpace(entity.ReferenceNo))
        {
            throw new ArgumentException("Procurement reference number is required.");
        }

        if (entity.ReferenceNo.Length > 50)
        {
            throw new ArgumentException("Procurement reference number cannot exceed 50 characters.");
        }

        if (entity.TotalAmount < 0)
        {
            throw new ArgumentException("Procurement total amount cannot be negative.");
        }

        if (entity.TotalAmount > 10000000) // Reasonable upper limit
        {
            throw new ArgumentException("Procurement total amount cannot exceed $10,000,000.");
        }

        if (entity.Notes?.Length > 1000)
        {
            throw new ArgumentException("Procurement notes cannot exceed 1000 characters.");
        }

        // Business rule: Reference number should follow a pattern (example)
        if (!System.Text.RegularExpressions.Regex.IsMatch(entity.ReferenceNo, @"^[A-Z]{2,3}-\d{4,6}$"))
        {
            throw new ArgumentException("Procurement reference number must follow the format: XX-NNNN or XXX-NNNNNN (e.g., PR-001234).");
        }
    }

    protected override string GenerateInsertQuery()
    {
        return @"
                INSERT INTO market.Procurement (VendorId, LocationId, ReferenceNo, ProcurementDate, TotalAmount, Notes, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
                VALUES (@VendorId, @LocationId, @ReferenceNo, @ProcurementDate, @TotalAmount, @Notes, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
    }

    protected override string GenerateUpdateQuery()
    {
        return @"
                UPDATE market.Procurement 
                SET VendorId = @VendorId, 
                    LocationId = @LocationId, 
                    ReferenceNo = @ReferenceNo, 
                    ProcurementDate = @ProcurementDate, 
                    TotalAmount = @TotalAmount, 
                    Notes = @Notes, 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
    }
}