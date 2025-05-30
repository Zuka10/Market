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
        var vendor = await connection.QueryFirstOrDefaultAsync<Vendor>(sql, new { Email = email });

        return vendor ?? throw new KeyNotFoundException($"Vendor with email '{email}' was not found.");
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

        var result = vendorDict.Values.FirstOrDefault();
        return result ?? throw new KeyNotFoundException($"Vendor with ID '{id}' was not found.");
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
        using var locationConnection = await _connectionFactory.CreateConnectionAsync();
        var locationCheckSql = "SELECT COUNT(1) FROM market.Location WHERE Id = @LocationId";
        var locationExists = await locationConnection.QuerySingleAsync<int>(locationCheckSql, new { LocationId = locationId });
        if (locationExists == 0)
        {
            throw new KeyNotFoundException($"Location with ID '{locationId}' was not found.");
        }

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

    public override async Task UpdateAsync(Vendor entity)
    {
        ValidateVendorRules(entity);

        await ValidateEmailUniqueness(entity);

        await base.UpdateAsync(entity);
    }

    public override async Task<Vendor> AddAsync(Vendor entity)
    {
        ValidateVendorRules(entity);

        if (await IsEmailExistsAsync(entity.Email))
        {
            throw new ArgumentException($"Vendor email '{entity.Email}' is already taken.");
        }

        return await base.AddAsync(entity);
    }

    public override async Task DeleteAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var activeLocationsCheckSql = @"
            SELECT COUNT(1) FROM market.VendorLocation 
            WHERE VendorId = @VendorId 
              AND IsActive = 1 
              AND StartDate <= GETUTCDATE() 
              AND (EndDate IS NULL OR EndDate > GETUTCDATE())";
        var hasActiveLocations = await connection.QuerySingleAsync<int>(activeLocationsCheckSql, new { VendorId = id });

        if (hasActiveLocations > 0)
        {
            throw new InvalidOperationException($"Cannot delete vendor with ID '{id}' because it has {hasActiveLocations} active location(s). Deactivate all locations first.");
        }

        var procurementsCheckSql = "SELECT COUNT(1) FROM market.Procurement WHERE VendorId = @VendorId";
        var hasProcurements = await connection.QuerySingleAsync<int>(procurementsCheckSql, new { VendorId = id });

        if (hasProcurements > 0)
        {
            throw new InvalidOperationException($"Cannot delete vendor with ID '{id}' because it has {hasProcurements} associated procurement(s). Deactivate the vendor instead.");
        }

        var discountsCheckSql = "SELECT COUNT(1) FROM market.Discount WHERE VendorId = @VendorId";
        var hasDiscounts = await connection.QuerySingleAsync<int>(discountsCheckSql, new { VendorId = id });

        if (hasDiscounts > 0)
        {
            throw new InvalidOperationException($"Cannot delete vendor with ID '{id}' because it has {hasDiscounts} associated discount(s). Deactivate the vendor instead.");
        }

        await base.DeleteAsync(id);
    }

    private static void ValidateVendorRules(Vendor entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            throw new ArgumentException("Vendor name is required.");
        }

        if (entity.Name.Length > 200)
        {
            throw new ArgumentException("Vendor name cannot exceed 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(entity.ContactPersonName))
        {
            throw new ArgumentException("Contact person name is required.");
        }

        if (entity.ContactPersonName.Length > 100)
        {
            throw new ArgumentException("Contact person name cannot exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(entity.Email))
        {
            throw new ArgumentException("Vendor email is required.");
        }

        if (entity.Email.Length > 100)
        {
            throw new ArgumentException("Vendor email cannot exceed 100 characters.");
        }

        if (!IsValidEmail(entity.Email))
        {
            throw new ArgumentException("Vendor email format is invalid.");
        }

        if (string.IsNullOrWhiteSpace(entity.PhoneNumber))
        {
            throw new ArgumentException("Phone number is required.");
        }

        if (entity.PhoneNumber.Length > 20)
        {
            throw new ArgumentException("Phone number cannot exceed 20 characters.");
        }

        // Validate phone number format (basic validation)
        if (!System.Text.RegularExpressions.Regex.IsMatch(entity.PhoneNumber, @"^[\+]?[0-9\s\-\(\)]+$"))
        {
            throw new ArgumentException("Phone number format is invalid. Use only numbers, spaces, hyphens, parentheses, and + sign.");
        }

        if (entity.Address?.Length > 500)
        {
            throw new ArgumentException("Address cannot exceed 500 characters.");
        }

        if (entity.Description?.Length > 1000)
        {
            throw new ArgumentException("Description cannot exceed 1000 characters.");
        }

        if (entity.CommissionRate < 0)
        {
            throw new ArgumentException("Commission rate cannot be negative.");
        }

        if (entity.CommissionRate > 100)
        {
            throw new ArgumentException("Commission rate cannot exceed 100%.");
        }

        // Business rule: Commission rate should be reasonable
        if (entity.CommissionRate > 50)
        {
            throw new ArgumentException("Commission rate cannot exceed 50% for business policy reasons.");
        }
    }

    private async Task ValidateEmailUniqueness(Vendor entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var emailCheckSql = $"SELECT COUNT(1) FROM {FullTableName} WHERE Email = @Email AND Id != @Id";
        var emailExists = await connection.QuerySingleAsync<int>(emailCheckSql, new { Email = entity.Email, Id = entity.Id });

        if (emailExists > 0)
        {
            throw new ArgumentException($"Vendor email '{entity.Email}' is already taken by another vendor.");
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
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