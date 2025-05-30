using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Market;
using Market.Infrastructure.Constants;

namespace Market.Infrastructure.Data.Repositories.Market;

public class CategoryRepository(IDbConnectionFactory connectionFactory) :
    GenericRepository<Category>(connectionFactory, DatabaseConstants.Tables.Market.Category, DatabaseConstants.Schemas.Market),
    ICategoryRepository
{
    public async Task<Category?> GetByNameAsync(string name)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE Name = @Name";
        var category = await connection.QueryFirstOrDefaultAsync<Category>(sql, new { Name = name });

        return category ?? throw new KeyNotFoundException($"Category with name '{name}' was not found.");
    }

    public async Task<Category?> GetCategoryWithProductsAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT c.*, p.Id, p.Name, p.Description, p.Price, p.InStock, p.Unit, p.IsAvailable
                FROM market.Category c
                LEFT JOIN market.Product p ON c.Id = p.CategoryId AND p.IsAvailable = 1
                WHERE c.Id = @Id";

        var categoryDict = new Dictionary<long, Category>();

        await connection.QueryAsync<Category, Product, Category>(
            sql,
            (category, product) =>
            {
                if (!categoryDict.TryGetValue(category.Id, out var existingCategory))
                {
                    existingCategory = category;
                    existingCategory.Products = new List<Product>();
                    categoryDict.Add(category.Id, existingCategory);
                }

                if (product != null)
                {
                    existingCategory.Products.Add(product);
                }

                return existingCategory;
            },
            new { Id = id },
            splitOn: "Id");

        var result = categoryDict.Values.FirstOrDefault();
        return result ?? throw new KeyNotFoundException($"Category with ID '{id}' was not found.");
    }

    public async Task<IEnumerable<Category>> GetCategoriesWithProductsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT c.*, p.Id, p.Name, p.Description, p.Price, p.InStock, p.Unit, p.IsAvailable
                FROM market.Category c
                LEFT JOIN market.Product p ON c.Id = p.CategoryId AND p.IsAvailable = 1
                ORDER BY c.Name";

        var categoryDict = new Dictionary<long, Category>();

        return await connection.QueryAsync<Category, Product, Category>(
            sql,
            (category, product) =>
            {
                if (!categoryDict.TryGetValue(category.Id, out var existingCategory))
                {
                    existingCategory = category;
                    existingCategory.Products = new List<Product>();
                    categoryDict.Add(category.Id, existingCategory);
                }

                if (product != null)
                {
                    existingCategory.Products.Add(product);
                }

                return existingCategory;
            },
            splitOn: "Id");
    }

    public async Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                SELECT * FROM {FullTableName} 
                WHERE Name LIKE @SearchTerm OR Description LIKE @SearchTerm";
        return await connection.QueryAsync<Category>(sql, new { SearchTerm = $"%{searchTerm}%" });
    }

    public async Task<int> GetProductCountByCategoryAsync(long categoryId)
    {
        if (!await ExistsAsync(categoryId))
        {
            throw new KeyNotFoundException($"Category with ID '{categoryId}' was not found.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM market.Product WHERE CategoryId = @CategoryId AND IsAvailable = 1";
        return await connection.QuerySingleAsync<int>(sql, new { CategoryId = categoryId });
    }

    public async Task<bool> IsNameExistsAsync(string name)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT COUNT(1) FROM {FullTableName} WHERE Name = @Name";
        var count = await connection.QuerySingleAsync<int>(sql, new { Name = name });
        return count > 0;
    }

    public override async Task UpdateAsync(Category entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var nameCheckSql = $"SELECT COUNT(1) FROM {FullTableName} WHERE Name = @Name AND Id != @Id";
        var nameExists = await connection.QuerySingleAsync<int>(nameCheckSql, new { entity.Name, entity.Id });
        if (nameExists > 0)
        {
            throw new ArgumentException($"Category name '{entity.Name}' is already taken by another category.");
        }

        await base.UpdateAsync(entity);
    }

    public override async Task<Category> AddAsync(Category entity)
    {
        if (await IsNameExistsAsync(entity.Name))
        {
            throw new ArgumentException($"Category name '{entity.Name}' is already taken.");
        }

        return await base.AddAsync(entity);
    }

    public override async Task DeleteAsync(long id)
    {
        var productCount = await GetProductCountByCategoryAsync(id);
        if (productCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete category with ID '{id}' because it has {productCount} associated product(s). Remove all products from this category first.");
        }

        await base.DeleteAsync(id);
    }

    protected override string GenerateInsertQuery()
    {
        return @"
                INSERT INTO market.Category (Name, Description, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
                VALUES (@Name, @Description, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
    }

    protected override string GenerateUpdateQuery()
    {
        return @"
                UPDATE market.Category 
                SET Name = @Name, 
                    Description = @Description, 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
    }
}