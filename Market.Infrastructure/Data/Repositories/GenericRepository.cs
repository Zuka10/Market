using Dapper;
using Market.Domain.Abstractions.Repositories;
using System.Data;
using System.Linq.Expressions;

namespace Market.Infrastructure.Data.Repositories;

public abstract class GenericRepository<T>(IDbConnectionFactory connectionFactory, string tableName, string schema = "dbo") :
    IGenericRepository<T> where T : class
{
    protected readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    protected readonly string _tableName = tableName;
    protected readonly string _schema = schema;

    protected string FullTableName => $"[{_schema}].[{_tableName}]";

    public virtual async Task<T?> GetByIdAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName}";
        return await connection.QueryAsync<T>(sql);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        // For simplicity, this returns all records. In a real implementation,
        // you would convert the expression to SQL or use a query builder
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName}";
        var allRecords = await connection.QueryAsync<T>(sql);
        return allRecords.Where(predicate.Compile());
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var insertQuery = GenerateInsertQuery();
        var id = await connection.QuerySingleAsync<long>(insertQuery, entity);

        // Set the ID property using reflection
        var idProperty = typeof(T).GetProperty("Id");
        idProperty?.SetValue(entity, id);

        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var updateQuery = GenerateUpdateQuery();
        await connection.ExecuteAsync(updateQuery, entity);
    }

    public virtual async Task DeleteAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"DELETE FROM {FullTableName} WHERE Id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public virtual async Task<int> CountAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT COUNT(*) FROM {FullTableName}";
        return await connection.QuerySingleAsync<int>(sql);
    }

    public virtual async Task<bool> ExistsAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT COUNT(1) FROM {FullTableName} WHERE Id = @Id";
        var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var countSql = $"SELECT COUNT(*) FROM {FullTableName}";
        var totalCount = await connection.QuerySingleAsync<int>(countSql);

        var offset = (page - 1) * pageSize;
        var sql = $@"
                SELECT * FROM {FullTableName}
                ORDER BY Id
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        var items = await connection.QueryAsync<T>(sql, new { Offset = offset, PageSize = pageSize });

        return (items, totalCount);
    }

    protected abstract string GenerateInsertQuery();
    protected abstract string GenerateUpdateQuery();
}