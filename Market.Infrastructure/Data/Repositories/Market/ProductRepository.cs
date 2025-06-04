using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Market;
using Market.Domain.Filters;
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

    public async Task<PagedResult<Product>> GetProductsAsync(ProductFilterParameters filterParams)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var (whereClause, parameters) = BuildWhereClause(filterParams);
        var orderByClause = BuildOrderByClause(filterParams.SortBy, filterParams.SortDirection);

        // Count query
        var countSql = BuildCountQuery(whereClause);
        var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

        // Decide on pagination strategy
        if (ShouldPaginate(filterParams, totalCount))
        {
            // Use database pagination for large results
            var (offset, pageSize) = CalculatePagination(filterParams.PageNumber, filterParams.PageSize);
            parameters.Add("Offset", offset);
            parameters.Add("PageSize", pageSize);

            var dataSql = BuildDataQueryWithPagination(whereClause, orderByClause);
            var products = await connection.QueryAsync<Product, Category, Location, int, int, Product>(
                dataSql,
                (product, category, location, orderCount, procurementCount) =>
                {
                    product.Category = category;
                    product.Location = location;
                    // OrderCount and ProcurementCount can be used for additional info if needed
                    return product;
                },
                parameters,
                splitOn: "CategoryId_Split,LocationId_Split,OrderCount,ProcurementCount"
            );

            var paginationMetadata = CalculatePaginationMetadata(totalCount, filterParams.PageNumber, filterParams.PageSize);

            return new PagedResult<Product>
            {
                Items = products.ToList(),
                TotalCount = totalCount,
                Page = filterParams.PageNumber,
                PageSize = filterParams.PageSize,
                TotalPages = paginationMetadata.TotalPages,
                HasNextPage = paginationMetadata.HasNextPage,
                HasPreviousPage = paginationMetadata.HasPreviousPage
            };
        }
        else
        {
            // Return all results for small datasets
            var dataSql = BuildDataQuery(whereClause, orderByClause);
            var products = await connection.QueryAsync<Product, Category, Location, Product>(
                dataSql,
                (product, category, location) =>
                {
                    product.Category = category;
                    product.Location = location;
                    return product;
                },
                parameters,
                splitOn: "CategoryId_Split,LocationId_Split"
            );

            var productsList = products.ToList();

            return new PagedResult<Product>
            {
                Items = productsList,
                TotalCount = totalCount,
                Page = 1,
                PageSize = productsList.Count,
                TotalPages = 1,
                HasNextPage = false,
                HasPreviousPage = false
            };
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

    private static (string WhereClause, DynamicParameters Parameters) BuildWhereClause(ProductFilterParameters filterParams)
    {
        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
        {
            whereConditions.Add("(p.Name LIKE @SearchTerm OR p.Description LIKE @SearchTerm OR c.Name LIKE @SearchTerm OR l.Name LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{filterParams.SearchTerm.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Name))
        {
            whereConditions.Add("p.Name LIKE @Name");
            parameters.Add("Name", $"%{filterParams.Name.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Description))
        {
            whereConditions.Add("p.Description LIKE @Description");
            parameters.Add("Description", $"%{filterParams.Description.Trim()}%");
        }

        if (filterParams.IsAvailable.HasValue)
        {
            whereConditions.Add("p.IsAvailable = @IsAvailable");
            parameters.Add("IsAvailable", filterParams.IsAvailable.Value);
        }

        if (filterParams.MinPrice.HasValue)
        {
            whereConditions.Add("p.Price >= @MinPrice");
            parameters.Add("MinPrice", filterParams.MinPrice.Value);
        }

        if (filterParams.MaxPrice.HasValue)
        {
            whereConditions.Add("p.Price <= @MaxPrice");
            parameters.Add("MaxPrice", filterParams.MaxPrice.Value);
        }

        if (filterParams.MinStock.HasValue)
        {
            whereConditions.Add("p.InStock >= @MinStock");
            parameters.Add("MinStock", filterParams.MinStock.Value);
        }

        if (filterParams.MaxStock.HasValue)
        {
            whereConditions.Add("p.InStock <= @MaxStock");
            parameters.Add("MaxStock", filterParams.MaxStock.Value);
        }

        if (filterParams.IsOutOfStock.HasValue && filterParams.IsOutOfStock.Value)
        {
            whereConditions.Add("p.InStock = 0");
        }

        if (filterParams.IsLowStock.HasValue && filterParams.IsLowStock.Value)
        {
            var threshold = filterParams.LowStockThreshold ?? 10;
            whereConditions.Add("p.InStock <= @LowStockThreshold AND p.InStock > 0");
            parameters.Add("LowStockThreshold", threshold);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Unit))
        {
            whereConditions.Add("p.Unit LIKE @Unit");
            parameters.Add("Unit", $"%{filterParams.Unit.Trim()}%");
        }

        if (filterParams.CategoryId.HasValue)
        {
            whereConditions.Add("p.CategoryId = @CategoryId");
            parameters.Add("CategoryId", filterParams.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.CategoryName))
        {
            whereConditions.Add("c.Name LIKE @CategoryName");
            parameters.Add("CategoryName", $"%{filterParams.CategoryName.Trim()}%");
        }

        if (filterParams.LocationId.HasValue)
        {
            whereConditions.Add("p.LocationId = @LocationId");
            parameters.Add("LocationId", filterParams.LocationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.LocationName))
        {
            whereConditions.Add("l.Name LIKE @LocationName");
            parameters.Add("LocationName", $"%{filterParams.LocationName.Trim()}%");
        }

        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";
        return (whereClause, parameters);
    }

    private static string BuildOrderByClause(string? sortBy, string? sortDirection)
    {
        var validSortFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "id", "p.Id" },
            { "name", "p.Name" },
            { "description", "p.Description" },
            { "price", "p.Price" },
            { "instock", "p.InStock" },
            { "unit", "p.Unit" },
            { "isavailable", "p.IsAvailable" },
            { "categoryname", "c.Name" },
            { "locationname", "l.Name" },
            { "createdat", "p.CreatedAt" },
            { "updatedat", "p.UpdatedAt" }
        };

        var sortField = validSortFields.ContainsKey(sortBy ?? "name")
            ? validSortFields[sortBy ?? "name"]
            : validSortFields["name"];

        var direction = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

        return $"ORDER BY {sortField} {direction}";
    }

    private static (int Offset, int PageSize) CalculatePagination(int pageNumber, int pageSize)
    {
        var offset = (pageNumber - 1) * pageSize;
        return (offset, pageSize);
    }

    private static bool ShouldPaginate(ProductFilterParameters filterParams, int totalCount)
    {
        // Use pagination when:
        // 1. Total count is large (>1000 records for product data)
        // 2. Specific page size is requested and reasonable
        // 3. Page number is greater than 1
        return totalCount > 1000 ||
               (filterParams.PageSize <= 100 && filterParams.PageNumber > 1) ||
               (filterParams.PageSize <= 50 && totalCount > 200);
    }

    private string BuildCountQuery(string whereClause)
    {
        return $@"
        SELECT COUNT(*)
        FROM {FullTableName} p
        INNER JOIN market.Category c ON p.CategoryId = c.Id
        INNER JOIN market.Location l ON p.LocationId = l.Id
        {whereClause}";
    }

    private string BuildDataQuery(string whereClause, string orderByClause)
    {
        return $@"
        SELECT p.*,
               c.Id as CategoryId_Split, c.Name as CategoryName, c.Description as CategoryDescription,
               l.Id as LocationId_Split, l.Name as LocationName, l.City, l.Address
        FROM {FullTableName} p
        INNER JOIN market.Category c ON p.CategoryId = c.Id
        INNER JOIN market.Location l ON p.LocationId = l.Id
        {whereClause}
        {orderByClause}";
    }

    private string BuildDataQueryWithPagination(string whereClause, string orderByClause)
    {
        return $@"
        SELECT p.*,
               c.Id as CategoryId_Split, c.Name as CategoryName, c.Description as CategoryDescription,
               l.Id as LocationId_Split, l.Name as LocationName, l.City, l.Address,
               ISNULL(od.OrderCount, 0) as OrderCount,
               ISNULL(pd.ProcurementCount, 0) as ProcurementCount
        FROM {FullTableName} p
        INNER JOIN market.Category c ON p.CategoryId = c.Id
        INNER JOIN market.Location l ON p.LocationId = l.Id
        LEFT JOIN (
            SELECT ProductId, COUNT(*) as OrderCount 
            FROM market.OrderDetail 
            GROUP BY ProductId
        ) od ON p.Id = od.ProductId
        LEFT JOIN (
            SELECT ProductId, COUNT(*) as ProcurementCount 
            FROM market.ProcurementDetail 
            GROUP BY ProductId
        ) pd ON p.Id = pd.ProductId
        {whereClause}
        {orderByClause}
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";
    }

    private static (int TotalPages, bool HasNextPage, bool HasPreviousPage) CalculatePaginationMetadata(
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var hasNextPage = pageNumber < totalPages;
        var hasPreviousPage = pageNumber > 1;

        return (totalPages, hasNextPage, hasPreviousPage);
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