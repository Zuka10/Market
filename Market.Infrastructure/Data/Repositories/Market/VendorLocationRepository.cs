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
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE VendorId = @VendorId";
        return await connection.QueryAsync<VendorLocation>(sql, new { VendorId = vendorId });
    }

    public async Task<IEnumerable<VendorLocation>> GetByLocationAsync(long locationId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE LocationId = @LocationId";
        return await connection.QueryAsync<VendorLocation>(sql, new { LocationId = locationId });
    }

    public async Task<VendorLocation?> GetVendorLocationAsync(long vendorId, long locationId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE VendorId = @VendorId AND LocationId = @LocationId";
        return await connection.QueryFirstOrDefaultAsync<VendorLocation>(sql, new { VendorId = vendorId, LocationId = locationId });
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