using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Market;
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