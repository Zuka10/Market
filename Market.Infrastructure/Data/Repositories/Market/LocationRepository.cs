using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Market;
using Market.Domain.Filters;
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

        var result = locationDict.Values.FirstOrDefault();
        return result ?? throw new KeyNotFoundException($"Location with ID '{id}' was not found.");
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
        if (!await ExistsAsync(locationId))
        {
            throw new KeyNotFoundException($"Location with ID '{locationId}' was not found.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM market.VendorLocation WHERE LocationId = @LocationId AND IsActive = 1";
        return await connection.QuerySingleAsync<int>(sql, new { LocationId = locationId });
    }
    public async Task<PagedResult<Location>> GetLocationsPagedAsync(LocationFilterParameters filterParams)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        // Build WHERE clause based on filters
        if (filterParams.IsActive.HasValue)
        {
            whereConditions.Add("l.IsActive = @IsActive");
            parameters.Add("IsActive", filterParams.IsActive.Value);
        }

        if (!string.IsNullOrEmpty(filterParams.SearchTerm))
        {
            whereConditions.Add("(l.Name LIKE @SearchTerm OR l.Address LIKE @SearchTerm OR l.City LIKE @SearchTerm OR l.State LIKE @SearchTerm OR l.Country LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{filterParams.SearchTerm}%");
        }

        if (!string.IsNullOrEmpty(filterParams.City))
        {
            whereConditions.Add("l.City LIKE @City");
            parameters.Add("City", $"%{filterParams.City}%");
        }

        if (!string.IsNullOrEmpty(filterParams.Country))
        {
            whereConditions.Add("l.Country LIKE @Country");
            parameters.Add("Country", $"%{filterParams.Country}%");
        }

        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";

        // Build ORDER BY clause
        var validSortFields = new Dictionary<string, string>
        {
            { "name", "l.Name" },
            { "city", "l.City" },
            { "country", "l.Country" },
            { "createdat", "l.CreatedAt" },
            { "updatedat", "l.UpdatedAt" }
        };

        var sortField = validSortFields.ContainsKey(filterParams.SortBy?.ToLower() ?? "name")
            ? validSortFields[filterParams.SortBy.ToLower()]
            : validSortFields["name"];

        var sortDirection = filterParams.SortDirection?.ToLower() == "desc" ? "DESC" : "ASC";
        var orderByClause = $"ORDER BY {sortField} {sortDirection}";

        // Calculate pagination
        var offset = (filterParams.PageNumber - 1) * filterParams.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filterParams.PageSize);

        // Count query
        var countSql = $@"
        SELECT COUNT(*)
        FROM {FullTableName} l
        {whereClause}";

        // Data query with vendor and product counts
        var dataSql = $@"
        SELECT l.*,
               ISNULL(v.VendorCount, 0) as VendorCount,
               ISNULL(p.ProductCount, 0) as ProductCount
        FROM {FullTableName} l
        LEFT JOIN (
            SELECT LocationId, COUNT(*) as VendorCount 
            FROM Vendors 
            GROUP BY LocationId
        ) v ON l.Id = v.LocationId
        LEFT JOIN (
            SELECT LocationId, COUNT(*) as ProductCount 
            FROM Products 
            GROUP BY LocationId
        ) p ON l.Id = p.LocationId
        {whereClause}
        {orderByClause}
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";

        // Execute queries
        var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

        var locations = await connection.QueryAsync<Location, int, int, Location>(
            dataSql,
            (location, vendorCount, productCount) =>
            {
                // Set the counts (these would be mapped to DTO properties)
                // You might want to store these in additional properties or handle in mapping
                return location;
            },
            parameters,
            splitOn: "VendorCount,ProductCount"
        );

        // Calculate pagination metadata
        var totalPages = (int)Math.Ceiling((double)totalCount / filterParams.PageSize);

        return new PagedResult<Location>
        {
            Items = locations.ToList(),
            TotalCount = totalCount,
            Page = filterParams.PageNumber,
            PageSize = filterParams.PageSize,
            TotalPages = totalPages,
            HasNextPage = filterParams.PageNumber < totalPages,
            HasPreviousPage = filterParams.PageNumber > 1
        };
    }

    public override async Task UpdateAsync(Location entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var duplicateCheckSql = $@"
            SELECT COUNT(1) FROM {FullTableName} 
            WHERE Name = @Name AND Address = @Address AND City = @City AND Id != @Id";

        var duplicateExists = await connection.QuerySingleAsync<int>(duplicateCheckSql,
            new { entity.Name, entity.Address, entity.City, entity.Id });

        if (duplicateExists > 0)
        {
            throw new ArgumentException($"A location with name '{entity.Name}' at address '{entity.Address}' in '{entity.City}' already exists.");
        }

        await base.UpdateAsync(entity);
    }

    public override async Task<Location> AddAsync(Location entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var duplicateCheckSql = $@"
            SELECT COUNT(1) FROM {FullTableName} 
            WHERE Name = @Name AND Address = @Address AND City = @City";

        var duplicateExists = await connection.QuerySingleAsync<int>(duplicateCheckSql,
            new { entity.Name, entity.Address, entity.City });

        if (duplicateExists > 0)
        {
            throw new ArgumentException($"A location with name '{entity.Name}' at address '{entity.Address}' in '{entity.City}' already exists.");
        }

        return await base.AddAsync(entity);
    }

    public override async Task DeleteAsync(long id)
    {
        var vendorCount = await GetVendorCountByLocationAsync(id);
        if (vendorCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete location with ID '{id}' because it has {vendorCount} active vendor(s). Remove all vendors from this location first.");
        }

        await base.DeleteAsync(id);
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