using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Market;
using Market.Infrastructure.Constants;

namespace Market.Infrastructure.Data.Repositories.Market;

public class VendorLocationRepository(IDbConnectionFactory connectionFactory) :
    GenericRepository<VendorLocation>(connectionFactory, DatabaseConstants.Tables.Market.VendorLocation, DatabaseConstants.Schemas.Market),
    IVendorLocationRepository
{
    public async Task<IEnumerable<VendorLocation>> GetActiveVendorLocationsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                SELECT * FROM {FullTableName} 
                WHERE IsActive = 1 
                  AND StartDate <= GETUTCDATE() 
                  AND (EndDate IS NULL OR EndDate > GETUTCDATE())";
        return await connection.QueryAsync<VendorLocation>(sql);
    }

    public async Task<IEnumerable<VendorLocation>> GetByVendorAsync(long vendorId)
    {
        // Validate that vendor exists
        using var vendorConnection = await _connectionFactory.CreateConnectionAsync();
        var vendorCheckSql = "SELECT COUNT(1) FROM market.Vendor WHERE Id = @VendorId";
        var vendorExists = await vendorConnection.QuerySingleAsync<int>(vendorCheckSql, new { VendorId = vendorId });
        if (vendorExists == 0)
        {
            throw new KeyNotFoundException($"Vendor with ID '{vendorId}' was not found.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE VendorId = @VendorId";
        return await connection.QueryAsync<VendorLocation>(sql, new { VendorId = vendorId });
    }

    public async Task<IEnumerable<VendorLocation>> GetByLocationAsync(long locationId)
    {
        using var locationConnection = await _connectionFactory.CreateConnectionAsync();
        var locationCheckSql = "SELECT COUNT(1) FROM market.Location WHERE Id = @LocationId";
        var locationExists = await locationConnection.QuerySingleAsync<int>(locationCheckSql, new { LocationId = locationId });
        if (locationExists == 0)
        {
            throw new KeyNotFoundException($"Location with ID '{locationId}' was not found.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE LocationId = @LocationId";
        return await connection.QueryAsync<VendorLocation>(sql, new { LocationId = locationId });
    }

    public async Task<VendorLocation?> GetVendorLocationAsync(long vendorId, long locationId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE VendorId = @VendorId AND LocationId = @LocationId";
        var vendorLocation = await connection.QueryFirstOrDefaultAsync<VendorLocation>(sql, new { VendorId = vendorId, LocationId = locationId });

        return vendorLocation ?? throw new KeyNotFoundException($"Vendor location relationship between vendor ID '{vendorId}' and location ID '{locationId}' was not found.");
    }

    public async Task<IEnumerable<VendorLocation>> GetVendorLocationsWithDetailsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT vl.*, v.Id, v.Name, v.ContactPersonName, v.Email, v.PhoneNumber,
                       l.Id, l.Name as LocationName, l.City, l.Address
                FROM market.VendorLocation vl
                INNER JOIN market.Vendor v ON vl.VendorId = v.Id
                INNER JOIN market.Location l ON vl.LocationId = l.Id
                ORDER BY v.Name, l.Name";

        var result = await connection.QueryAsync<VendorLocation, Vendor, Location, VendorLocation>(
            sql,
            (vendorLocation, vendor, location) =>
            {
                vendorLocation.Vendor = vendor;
                vendorLocation.Location = location;
                return vendorLocation;
            },
            splitOn: "Id,Id");

        return result;
    }

    public async Task<bool> IsVendorInLocationAsync(long vendorId, long locationId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                SELECT COUNT(1) FROM {FullTableName} 
                WHERE VendorId = @VendorId 
                  AND LocationId = @LocationId 
                  AND IsActive = 1 
                  AND StartDate <= GETUTCDATE() 
                  AND (EndDate IS NULL OR EndDate > GETUTCDATE())";
        var count = await connection.QuerySingleAsync<int>(sql, new { VendorId = vendorId, LocationId = locationId });
        return count > 0;
    }

    public override async Task UpdateAsync(VendorLocation entity)
    {
        await ValidateForeignKeys(entity);

        ValidateVendorLocationRules(entity);

        ValidateDateConstraints(entity);

        await ValidateStallNumberUniqueness(entity);

        await base.UpdateAsync(entity);
    }

    public override async Task<VendorLocation> AddAsync(VendorLocation entity)
    {
        await ValidateForeignKeys(entity);

        ValidateVendorLocationRules(entity);

        ValidateDateConstraints(entity);

        await ValidateStallNumberUniqueness(entity);

        await ValidateOverlappingRelationships(entity);

        return await base.AddAsync(entity);
    }

    public override async Task DeleteAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var vendorLocationSql = $"SELECT VendorId, LocationId FROM {FullTableName} WHERE Id = @Id";
        var vendorLocationInfo = await connection.QueryFirstAsync<dynamic>(vendorLocationSql, new { Id = id });

        var procurementCheckSql = "SELECT COUNT(1) FROM market.Procurement WHERE VendorId = @VendorId AND LocationId = @LocationId";
        var hasProcurements = await connection.QuerySingleAsync<int>(procurementCheckSql,
            new { vendorLocationInfo.VendorId, vendorLocationInfo.LocationId });

        if (hasProcurements > 0)
        {
            throw new InvalidOperationException($"Cannot delete vendor location with ID '{id}' because it has associated procurement records. Deactivate the relationship instead.");
        }

        await base.DeleteAsync(id);
    }

    private async Task ValidateForeignKeys(VendorLocation entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var vendorCheckSql = "SELECT COUNT(1) FROM market.Vendor WHERE Id = @VendorId AND IsActive = 1";
        var vendorExists = await connection.QuerySingleAsync<int>(vendorCheckSql, new { VendorId = entity.VendorId });
        if (vendorExists == 0)
        {
            throw new ArgumentException($"Vendor with ID '{entity.VendorId}' does not exist or is not active.");
        }

        var locationCheckSql = "SELECT COUNT(1) FROM market.Location WHERE Id = @LocationId AND IsActive = 1";
        var locationExists = await connection.QuerySingleAsync<int>(locationCheckSql, new { LocationId = entity.LocationId });
        if (locationExists == 0)
        {
            throw new ArgumentException($"Location with ID '{entity.LocationId}' does not exist or is not active.");
        }
    }

    private static void ValidateVendorLocationRules(VendorLocation entity)
    {
        if (entity.RentAmount < 0)
        {
            throw new ArgumentException("Rent amount cannot be negative.");
        }

        if (entity.RentAmount > 1000000)
        {
            throw new ArgumentException("Rent amount cannot exceed $1,000,000.");
        }

        if (string.IsNullOrWhiteSpace(entity.StallNumber))
        {
            throw new ArgumentException("Stall number is required.");
        }

        if (entity.StallNumber.Length > 20)
        {
            throw new ArgumentException("Stall number cannot exceed 20 characters.");
        }

        // Validate stall number format (example: should be alphanumeric)
        if (!System.Text.RegularExpressions.Regex.IsMatch(entity.StallNumber, @"^[A-Za-z0-9-]+$"))
        {
            throw new ArgumentException("Stall number must contain only letters, numbers, and hyphens.");
        }

        // Business rule: End date should be after start date
        if (entity.EndDate.HasValue && entity.EndDate <= entity.StartDate)
        {
            throw new ArgumentException("End date must be after start date.");
        }
    }

    private static void ValidateDateConstraints(VendorLocation entity)
    {
        // Start date cannot be too far in the past
        if (entity.StartDate < DateTime.UtcNow.AddYears(-5))
        {
            throw new ArgumentException("Start date cannot be more than 5 years in the past.");
        }

        // Start date cannot be too far in the future
        if (entity.StartDate > DateTime.UtcNow.AddYears(1))
        {
            throw new ArgumentException("Start date cannot be more than 1 year in the future.");
        }

        // End date cannot be too far in the future
        if (entity.EndDate.HasValue && entity.EndDate > DateTime.UtcNow.AddYears(10))
        {
            throw new ArgumentException("End date cannot be more than 10 years in the future.");
        }
    }

    private async Task ValidateStallNumberUniqueness(VendorLocation entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        // Check for stall number uniqueness within the same location and overlapping time periods
        var stallCheckSql = entity.Id > 0
            ? $@"SELECT COUNT(1) FROM {FullTableName} 
                 WHERE LocationId = @LocationId 
                   AND StallNumber = @StallNumber 
                   AND Id != @Id
                   AND IsActive = 1
                   AND StartDate <= @EndDate 
                   AND (EndDate IS NULL OR EndDate >= @StartDate)"
            : $@"SELECT COUNT(1) FROM {FullTableName} 
                 WHERE LocationId = @LocationId 
                   AND StallNumber = @StallNumber 
                   AND IsActive = 1
                   AND StartDate <= @EndDate 
                   AND (EndDate IS NULL OR EndDate >= @StartDate)";

        var endDate = entity.EndDate ?? DateTime.UtcNow.AddYears(100); // Use far future if no end date
        var stallExists = await connection.QuerySingleAsync<int>(stallCheckSql, new
        {
            entity.LocationId,
            entity.StallNumber,
            entity.StartDate,
            EndDate = endDate,
            entity.Id
        });

        if (stallExists > 0)
        {
            throw new ArgumentException($"Stall number '{entity.StallNumber}' is already occupied in location '{entity.LocationId}' during the specified time period.");
        }
    }

    private async Task ValidateOverlappingRelationships(VendorLocation entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        // Check if vendor already has an active relationship with this location during overlapping period
        var overlapCheckSql = $@"SELECT COUNT(1) FROM {FullTableName} 
                                WHERE VendorId = @VendorId 
                                  AND LocationId = @LocationId 
                                  AND IsActive = 1
                                  AND StartDate <= @EndDate 
                                  AND (EndDate IS NULL OR EndDate >= @StartDate)";

        var endDate = entity.EndDate ?? DateTime.UtcNow.AddYears(100); // Use far future if no end date
        var hasOverlap = await connection.QuerySingleAsync<int>(overlapCheckSql, new
        {
            entity.VendorId,
            entity.LocationId,
            entity.StartDate,
            EndDate = endDate
        });

        if (hasOverlap > 0)
        {
            throw new ArgumentException($"Vendor '{entity.VendorId}' already has an active relationship with location '{entity.LocationId}' during the specified time period.");
        }
    }

    protected override string GenerateInsertQuery()
    {
        return @"
                INSERT INTO market.VendorLocation (VendorId, LocationId, StallNumber, RentAmount, IsActive, StartDate, EndDate, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
                VALUES (@VendorId, @LocationId, @StallNumber, @RentAmount, @IsActive, @StartDate, @EndDate, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
    }

    protected override string GenerateUpdateQuery()
    {
        return @"
                UPDATE market.VendorLocation 
                SET VendorId = @VendorId, 
                    LocationId = @LocationId, 
                    StallNumber = @StallNumber, 
                    RentAmount = @RentAmount, 
                    IsActive = @IsActive, 
                    StartDate = @StartDate, 
                    EndDate = @EndDate, 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
    }
}