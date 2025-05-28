using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Market;
using Market.Infrastructure.Constants;

namespace Market.Infrastructure.Data.Repositories.Market;

public class VendorRepository(IDbConnectionFactory connectionFactory) :
    GenericRepository<Vendor>(connectionFactory, DatabaseConstants.Tables.Market.Vendor, DatabaseConstants.Schemas.Market),
    IVendorRepository
{
    public async Task<IEnumerable<Vendor>> GetActiveVendorsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE IsActive = 1";
        return await connection.QueryAsync<Vendor>(sql);
    }

    public async Task<Vendor?> GetByEmailAsync(string email)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE Email = @Email";
        return await connection.QueryFirstOrDefaultAsync<Vendor>(sql, new { Email = email });
    }

    public async Task<Vendor?> GetVendorWithLocationsAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT v.*, vl.Id, vl.VendorId, vl.LocationId, vl.StallNumber, vl.RentAmount, vl.IsActive, vl.StartDate, vl.EndDate,
                       l.Id, l.Name, l.Address, l.City, l.Country
                FROM market.Vendor v
                LEFT JOIN market.VendorLocation vl ON v.Id = vl.VendorId AND vl.IsActive = 1
                LEFT JOIN market.Location l ON vl.LocationId = l.Id
                WHERE v.Id = @Id";

        var vendorDict = new Dictionary<long, Vendor>();

        await connection.QueryAsync<Vendor, VendorLocation, Location, Vendor>(
            sql,
            (vendor, vendorLocation, location) =>
            {
                if (!vendorDict.TryGetValue(vendor.Id, out var existingVendor))
                {
                    existingVendor = vendor;
                    existingVendor.VendorLocations = new List<VendorLocation>();
                    vendorDict.Add(vendor.Id, existingVendor);
                }

                if (vendorLocation != null)
                {
                    vendorLocation.Location = location;
                    existingVendor.VendorLocations.Add(vendorLocation);
                }

                return existingVendor;
            },
            new { Id = id },
            splitOn: "Id,Id");

        return vendorDict.Values.FirstOrDefault();
    }

    public async Task<IEnumerable<Vendor>> GetVendorsWithLocationsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT v.*, vl.Id, vl.VendorId, vl.LocationId, vl.StallNumber, vl.RentAmount, vl.IsActive, vl.StartDate, vl.EndDate,
                       l.Id, l.Name, l.Address, l.City, l.Country
                FROM market.Vendor v
                LEFT JOIN market.VendorLocation vl ON v.Id = vl.VendorId AND vl.IsActive = 1
                LEFT JOIN market.Location l ON vl.LocationId = l.Id
                WHERE v.IsActive = 1";

        var vendorDict = new Dictionary<long, Vendor>();

        return await connection.QueryAsync<Vendor, VendorLocation, Location, Vendor>(
            sql,
            (vendor, vendorLocation, location) =>
            {
                if (!vendorDict.TryGetValue(vendor.Id, out var existingVendor))
                {
                    existingVendor = vendor;
                    existingVendor.VendorLocations = new List<VendorLocation>();
                    vendorDict.Add(vendor.Id, existingVendor);
                }

                if (vendorLocation != null)
                {
                    vendorLocation.Location = location;
                    existingVendor.VendorLocations.Add(vendorLocation);
                }

                return existingVendor;
            },
            splitOn: "Id,Id");
    }

    public async Task<IEnumerable<Vendor>> SearchVendorsAsync(string searchTerm)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                SELECT * FROM {FullTableName} 
                WHERE (Name LIKE @SearchTerm OR ContactPersonName LIKE @SearchTerm OR Email LIKE @SearchTerm) 
                  AND IsActive = 1";
        return await connection.QueryAsync<Vendor>(sql, new { SearchTerm = $"%{searchTerm}%" });
    }

    public async Task<IEnumerable<Vendor>> GetVendorsByLocationAsync(long locationId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT v.* FROM market.Vendor v
                INNER JOIN market.VendorLocation vl ON v.Id = vl.VendorId
                WHERE vl.LocationId = @LocationId AND vl.IsActive = 1 AND v.IsActive = 1";
        return await connection.QueryAsync<Vendor>(sql, new { LocationId = locationId });
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT COUNT(1) FROM {FullTableName} WHERE Email = @Email";
        var count = await connection.QuerySingleAsync<int>(sql, new { Email = email });
        return count > 0;
    }

    protected override string GenerateInsertQuery()
    {
        return @"
                INSERT INTO market.Vendor (Name, ContactPersonName, Email, PhoneNumber, Address, Description, CommissionRate, IsActive, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
                VALUES (@Name, @ContactPersonName, @Email, @PhoneNumber, @Address, @Description, @CommissionRate, @IsActive, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
    }

    protected override string GenerateUpdateQuery()
    {
        return @"
                UPDATE market.Vendor 
                SET Name = @Name, 
                    ContactPersonName = @ContactPersonName, 
                    Email = @Email, 
                    PhoneNumber = @PhoneNumber, 
                    Address = @Address, 
                    Description = @Description, 
                    CommissionRate = @CommissionRate, 
                    IsActive = @IsActive, 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
    }
}