using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Market;
using Market.Infrastructure.Constants;

namespace Market.Infrastructure.Data.Repositories.Market;

public class LocationRepository(IDbConnectionFactory connectionFactory) :
    GenericRepository<Location>(connectionFactory, DatabaseConstants.Tables.Market.Location, DatabaseConstants.Schemas.Market),
    ILocationRepository
{
    public async Task<IEnumerable<Location>> GetActiveLocationsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE IsActive = 1";
        return await connection.QueryAsync<Location>(sql);
    }

    public async Task<IEnumerable<Location>> GetLocationsByCityAsync(string city)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE City = @City AND IsActive = 1";
        return await connection.QueryAsync<Location>(sql, new { City = city });
    }

    public async Task<Location?> GetLocationWithVendorsAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT l.*, vl.Id, vl.VendorId, vl.LocationId, vl.StallNumber, vl.RentAmount, vl.IsActive, vl.StartDate, vl.EndDate,
                       v.Id, v.Name, v.ContactPersonName, v.Email, v.PhoneNumber
                FROM market.Location l
                LEFT JOIN market.VendorLocation vl ON l.Id = vl.LocationId AND vl.IsActive = 1
                LEFT JOIN market.Vendor v ON vl.VendorId = v.Id
                WHERE l.Id = @Id";

        var locationDict = new Dictionary<long, Location>();

        await connection.QueryAsync<Location, VendorLocation, Vendor, Location>(
            sql,
            (location, vendorLocation, vendor) =>
            {
                if (!locationDict.TryGetValue(location.Id, out var existingLocation))
                {
                    existingLocation = location;
                    existingLocation.VendorLocations = new List<VendorLocation>();
                    locationDict.Add(location.Id, existingLocation);
                }

                if (vendorLocation != null)
                {
                    vendorLocation.Vendor = vendor;
                    existingLocation.VendorLocations.Add(vendorLocation);
                }

                return existingLocation;
            },
            new { Id = id },
            splitOn: "Id,Id");

        return locationDict.Values.FirstOrDefault();
    }

    public async Task<IEnumerable<Location>> GetLocationsWithVendorsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT l.*, vl.Id, vl.VendorId, vl.LocationId, vl.StallNumber, vl.RentAmount, vl.IsActive, vl.StartDate, vl.EndDate,
                       v.Id, v.Name, v.ContactPersonName, v.Email, v.PhoneNumber
                FROM market.Location l
                LEFT JOIN market.VendorLocation vl ON l.Id = vl.LocationId AND vl.IsActive = 1
                LEFT JOIN market.Vendor v ON vl.VendorId = v.Id
                WHERE l.IsActive = 1";

        var locationDict = new Dictionary<long, Location>();

        return await connection.QueryAsync<Location, VendorLocation, Vendor, Location>(
            sql,
            (location, vendorLocation, vendor) =>
            {
                if (!locationDict.TryGetValue(location.Id, out var existingLocation))
                {
                    existingLocation = location;
                    existingLocation.VendorLocations = new List<VendorLocation>();
                    locationDict.Add(location.Id, existingLocation);
                }

                if (vendorLocation != null)
                {
                    vendorLocation.Vendor = vendor;
                    existingLocation.VendorLocations.Add(vendorLocation);
                }

                return existingLocation;
            },
            splitOn: "Id,Id");
    }

    public async Task<IEnumerable<Location>> SearchLocationsAsync(string searchTerm)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                SELECT * FROM {FullTableName} 
                WHERE (Name LIKE @SearchTerm OR City LIKE @SearchTerm OR Address LIKE @SearchTerm) 
                  AND IsActive = 1";
        return await connection.QueryAsync<Location>(sql, new { SearchTerm = $"%{searchTerm}%" });
    }

    public async Task<int> GetVendorCountByLocationAsync(long locationId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM market.VendorLocation WHERE LocationId = @LocationId AND IsActive = 1";
        return await connection.QuerySingleAsync<int>(sql, new { LocationId = locationId });
    }

    protected override string GenerateInsertQuery()
    {
        return @"
                INSERT INTO market.Location (Name, Address, City, PostalCode, Country, Phone, OpeningHours, Description, IsActive, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
                VALUES (@Name, @Address, @City, @PostalCode, @Country, @Phone, @OpeningHours, @Description, @IsActive, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
    }

    protected override string GenerateUpdateQuery()
    {
        return @"
                UPDATE market.Location 
                SET Name = @Name, 
                    Address = @Address, 
                    City = @City, 
                    PostalCode = @PostalCode, 
                    Country = @Country, 
                    Phone = @Phone, 
                    OpeningHours = @OpeningHours, 
                    Description = @Description, 
                    IsActive = @IsActive, 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
    }
}