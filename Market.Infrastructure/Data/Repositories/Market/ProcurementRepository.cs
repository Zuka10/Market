using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Market;
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

    public async Task<(IEnumerable<Procurement> Procurements, int TotalCount)> GetPagedProcurementsAsync(int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var countSql = $"SELECT COUNT(*) FROM {FullTableName}";
        var totalCount = await connection.QuerySingleAsync<int>(countSql);

        var offset = (page - 1) * pageSize;
        var sql = $@"
                SELECT * FROM {FullTableName}
                ORDER BY ProcurementDate DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        var procurements = await connection.QueryAsync<Procurement>(sql,
            new { Offset = offset, PageSize = pageSize });

        return (procurements, totalCount);
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