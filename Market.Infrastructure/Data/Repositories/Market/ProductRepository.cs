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
        using var categoryConnection = await _connectionFactory.CreateConnectionAsync();
        var categoryCheckSql = "SELECT COUNT(1) FROM market.Category WHERE Id = @CategoryId";
        var categoryExists = await categoryConnection.QuerySingleAsync<int>(categoryCheckSql, new { CategoryId = categoryId });
        if (categoryExists == 0)
        {
            throw new KeyNotFoundException($"Category with ID '{categoryId}' was not found.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE CategoryId = @CategoryId AND IsAvailable = 1";
        return await connection.QueryAsync<Product>(sql, new { CategoryId = categoryId });
    }

    public async Task<IEnumerable<Product>> GetProductsByLocationAsync(long locationId)
    {
        using var locationConnection = await _connectionFactory.CreateConnectionAsync();
        var locationCheckSql = "SELECT COUNT(1) FROM market.Location WHERE Id = @LocationId";
        var locationExists = await locationConnection.QuerySingleAsync<int>(locationCheckSql, new { LocationId = locationId });
        if (locationExists == 0)
        {
            throw new KeyNotFoundException($"Location with ID '{locationId}' was not found.");
        }

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

        var product = result.FirstOrDefault();
        return product ?? throw new KeyNotFoundException($"Product with ID '{id}' was not found.");
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
        if (!await ExistsAsync(productId))
        {
            throw new KeyNotFoundException($"Product with ID '{productId}' was not found and cannot update stock.");
        }

        if (newStock < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                UPDATE {FullTableName} 
                SET InStock = @NewStock, UpdatedAt = GETUTCDATE() 
                WHERE Id = @ProductId";

        var rowsAffected = await connection.ExecuteAsync(sql, new { ProductId = productId, NewStock = newStock });

        if (rowsAffected == 0)
        {
            throw new KeyNotFoundException($"Product with ID '{productId}' was not found and cannot update stock.");
        }
    }

    public override async Task UpdateAsync(Product entity)
    {
        await ValidateForeignKeys(entity);

        ValidateProductRules(entity);

        await ValidateProductUniqueness(entity);

        await base.UpdateAsync(entity);
    }

    public override async Task<Product> AddAsync(Product entity)
    {
        await ValidateForeignKeys(entity);

        ValidateProductRules(entity);

        await ValidateProductUniqueness(entity);

        return await base.AddAsync(entity);
    }

    public override async Task DeleteAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var orderDetailsCheckSql = "SELECT COUNT(1) FROM market.OrderDetail WHERE ProductId = @ProductId";
        var hasOrderDetails = await connection.QuerySingleAsync<int>(orderDetailsCheckSql, new { ProductId = id });
        if (hasOrderDetails > 0)
        {
            throw new InvalidOperationException($"Cannot delete product with ID '{id}' because it is referenced in {hasOrderDetails} order detail(s). Deactivate the product instead.");
        }

        var procurementDetailsCheckSql = "SELECT COUNT(1) FROM market.ProcurementDetail WHERE ProductId = @ProductId";
        var hasProcurementDetails = await connection.QuerySingleAsync<int>(procurementDetailsCheckSql, new { ProductId = id });
        if (hasProcurementDetails > 0)
        {
            throw new InvalidOperationException($"Cannot delete product with ID '{id}' because it is referenced in {hasProcurementDetails} procurement detail(s). Deactivate the product instead.");
        }

        await base.DeleteAsync(id);
    }

    private async Task ValidateForeignKeys(Product entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var categoryCheckSql = "SELECT COUNT(1) FROM market.Category WHERE Id = @CategoryId";
        var categoryExists = await connection.QuerySingleAsync<int>(categoryCheckSql, new { CategoryId = entity.CategoryId });
        if (categoryExists == 0)
        {
            throw new ArgumentException($"Category with ID '{entity.CategoryId}' does not exist.");
        }

        var locationCheckSql = "SELECT COUNT(1) FROM market.Location WHERE Id = @LocationId AND IsActive = 1";
        var locationExists = await connection.QuerySingleAsync<int>(locationCheckSql, new { LocationId = entity.LocationId });
        if (locationExists == 0)
        {
            throw new ArgumentException($"Location with ID '{entity.LocationId}' does not exist or is not active.");
        }
    }

    private static void ValidateProductRules(Product entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            throw new ArgumentException("Product name is required.");
        }

        if (entity.Name.Length > 200)
        {
            throw new ArgumentException("Product name cannot exceed 200 characters.");
        }

        if (entity.Description?.Length > 1000)
        {
            throw new ArgumentException("Product description cannot exceed 1000 characters.");
        }

        if (entity.Price < 0)
        {
            throw new ArgumentException("Product price cannot be negative.");
        }

        if (entity.Price > 1000000)
        {
            throw new ArgumentException("Product price cannot exceed $1,000,000.");
        }

        if (entity.InStock < 0)
        {
            throw new ArgumentException("Product stock cannot be negative.");
        }

        if (entity.InStock > 1000000)
        {
            throw new ArgumentException("Product stock cannot exceed 1,000,000 units.");
        }

        if (string.IsNullOrWhiteSpace(entity.Unit))
        {
            throw new ArgumentException("Product unit is required.");
        }

        if (entity.Unit.Length > 20)
        {
            throw new ArgumentException("Product unit cannot exceed 20 characters.");
        }

        // Validate unit format (should be standard units)
        var validUnits = new[] { "piece", "kg", "gram", "liter", "ml", "meter", "cm", "pack", "box", "dozen", "pair" };
        if (!validUnits.Contains(entity.Unit.ToLowerInvariant()))
        {
            throw new ArgumentException($"Product unit '{entity.Unit}' is not valid. Valid units are: {string.Join(", ", validUnits)}.");
        }
    }

    private async Task ValidateProductUniqueness(Product entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var duplicateCheckSql = entity.Id > 0
            ? $"SELECT COUNT(1) FROM {FullTableName} WHERE Name = @Name AND LocationId = @LocationId AND CategoryId = @CategoryId AND Id != @Id"
            : $"SELECT COUNT(1) FROM {FullTableName} WHERE Name = @Name AND LocationId = @LocationId AND CategoryId = @CategoryId";

        var duplicateExists = await connection.QuerySingleAsync<int>(duplicateCheckSql, new
        {
            entity.Name,
            entity.LocationId,
            entity.CategoryId,
            entity.Id
        });

        if (duplicateExists > 0)
        {
            throw new ArgumentException($"A product with name '{entity.Name}' already exists in the same location and category.");
        }
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