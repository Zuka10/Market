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
        return await connection.QueryFirstOrDefaultAsync<Discount>(sql, new { Code = code });
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