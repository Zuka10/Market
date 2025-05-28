using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Market;
using Market.Infrastructure.Constants;

namespace Market.Infrastructure.Data.Repositories.Market;

public class ProductRepository(IDbConnectionFactory connectionFactory) :
    GenericRepository<Product>(connectionFactory, DatabaseConstants.Tables.Market.Product, DatabaseConstants.Schemas.Market),
    IProductRepository
{
    public async Task<IEnumerable<Product>> GetAvailableProductsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE IsAvailable = 1";
        return await connection.QueryAsync<Product>(sql);
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(long categoryId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE CategoryId = @CategoryId AND IsAvailable = 1";
        return await connection.QueryAsync<Product>(sql, new { CategoryId = categoryId });
    }

    public async Task<IEnumerable<Product>> GetProductsByLocationAsync(long locationId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE LocationId = @LocationId AND IsAvailable = 1";
        return await connection.QueryAsync<Product>(sql, new { LocationId = locationId });
    }

    public async Task<Product?> GetProductWithDetailsAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT p.*, c.Id, c.Name, c.Description as CategoryDescription,
                       l.Id, l.Name, l.City, l.Address
                FROM market.Product p
                INNER JOIN market.Category c ON p.CategoryId = c.Id
                INNER JOIN market.Location l ON p.LocationId = l.Id
                WHERE p.Id = @Id";

        var result = await connection.QueryAsync<Product, Category, Location, Product>(
            sql,
            (product, category, location) =>
            {
                product.Category = category;
                product.Location = location;
                return product;
            },
            new { Id = id },
            splitOn: "Id,Id");

        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<Product>> GetProductsWithDetailsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT p.*, c.Id, c.Name, c.Description as CategoryDescription,
                       l.Id, l.Name, l.City, l.Address
                FROM market.Product p
                INNER JOIN market.Category c ON p.CategoryId = c.Id
                INNER JOIN market.Location l ON p.LocationId = l.Id
                WHERE p.IsAvailable = 1";

        return await connection.QueryAsync<Product, Category, Location, Product>(
            sql,
            (product, category, location) =>
            {
                product.Category = category;
                product.Location = location;
                return product;
            },
            splitOn: "Id,Id");
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                SELECT * FROM {FullTableName} 
                WHERE (Name LIKE @SearchTerm OR Description LIKE @SearchTerm) 
                  AND IsAvailable = 1";
        return await connection.QueryAsync<Product>(sql, new { SearchTerm = $"%{searchTerm}%" });
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                SELECT * FROM {FullTableName} 
                WHERE InStock <= @Threshold AND InStock > 0 AND IsAvailable = 1";
        return await connection.QueryAsync<Product>(sql, new { Threshold = threshold });
    }

    public async Task<IEnumerable<Product>> GetOutOfStockProductsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE InStock = 0 AND IsAvailable = 1";
        return await connection.QueryAsync<Product>(sql);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedProductsAsync(int page, int pageSize, bool? isAvailable = null)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var whereClause = isAvailable.HasValue ? "WHERE IsAvailable = @IsAvailable" : "";
        var countSql = $"SELECT COUNT(*) FROM {FullTableName} {whereClause}";
        var totalCount = await connection.QuerySingleAsync<int>(countSql, new { IsAvailable = isAvailable });

        var offset = (page - 1) * pageSize;
        var sql = $@"
                SELECT * FROM {FullTableName} {whereClause}
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        var products = await connection.QueryAsync<Product>(sql,
            new { IsAvailable = isAvailable, Offset = offset, PageSize = pageSize });

        return (products, totalCount);
    }

    public async Task UpdateStockAsync(long productId, int newStock)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                UPDATE {FullTableName} 
                SET InStock = @NewStock, UpdatedAt = GETUTCDATE() 
                WHERE Id = @ProductId";
        await connection.ExecuteAsync(sql, new { ProductId = productId, NewStock = newStock });
    }

    protected override string GenerateInsertQuery()
    {
        return @"
                INSERT INTO market.Product (Name, Description, Price, InStock, Unit, LocationId, CategoryId, IsAvailable, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
                VALUES (@Name, @Description, @Price, @InStock, @Unit, @LocationId, @CategoryId, @IsAvailable, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
    }

    protected override string GenerateUpdateQuery()
    {
        return @"
                UPDATE market.Product 
                SET Name = @Name, 
                    Description = @Description, 
                    Price = @Price, 
                    InStock = @InStock, 
                    Unit = @Unit, 
                    LocationId = @LocationId, 
                    CategoryId = @CategoryId, 
                    IsAvailable = @IsAvailable, 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
    }
}