using Dapper;
using Market.Domain.Abstractions.Repositories.Auth;
using Market.Domain.Entities.Auth;
using Market.Infrastructure.Constants;

namespace Market.Infrastructure.Data.Repositories.Auth;

public class UserRepository(IDbConnectionFactory connectionFactory) :
    GenericRepository<User>(connectionFactory, DatabaseConstants.Tables.Auth.User, DatabaseConstants.Schemas.Auth),
    IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE Username = @Username";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE Email = @Email";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User?> GetUserWithRoleAsync(long id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT u.*, r.Id, r.Name
                FROM auth.[User] u
                INNER JOIN auth.Role r ON u.RoleId = r.Id
                WHERE u.Id = @Id";

        var result = await connection.QueryAsync<User, Role, User>(
            sql,
            (user, role) =>
            {
                user.Role = role;
                return user;
            },
            new { Id = id },
            splitOn: "Id");

        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(long roleId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE RoleId = @RoleId";
        return await connection.QueryAsync<User>(sql, new { RoleId = roleId });
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE IsActive = 1";
        return await connection.QueryAsync<User>(sql);
    }

    public async Task<bool> IsUsernameExistsAsync(string username)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT COUNT(1) FROM {FullTableName} WHERE Username = @Username";
        var count = await connection.QuerySingleAsync<int>(sql, new { Username = username });
        return count > 0;
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT COUNT(1) FROM {FullTableName} WHERE Email = @Email";
        var count = await connection.QuerySingleAsync<int>(sql, new { Email = email });
        return count > 0;
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                SELECT * FROM {FullTableName} 
                WHERE Username LIKE @SearchTerm 
                   OR Email LIKE @SearchTerm 
                   OR FirstName LIKE @SearchTerm 
                   OR LastName LIKE @SearchTerm";

        return await connection.QueryAsync<User>(sql, new { SearchTerm = $"%{searchTerm}%" });
    }

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersAsync(int page, int pageSize, bool? isActive = null)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var whereClause = isActive.HasValue ? "WHERE IsActive = @IsActive" : "";
        var countSql = $"SELECT COUNT(*) FROM {FullTableName} {whereClause}";
        var totalCount = await connection.QuerySingleAsync<int>(countSql, new { IsActive = isActive });

        var offset = (page - 1) * pageSize;
        var sql = $@"
                SELECT * FROM {FullTableName} {whereClause}
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        var users = await connection.QueryAsync<User>(sql,
            new { IsActive = isActive, Offset = offset, PageSize = pageSize });

        return (users, totalCount);
    }

    protected override string GenerateInsertQuery()
    {
        return @"
                INSERT INTO auth.[User] (Username, Email, PasswordHash, FirstName, LastName, PhoneNumber, RoleId, IsActive, CreatedAt, UpdatedAt)
                VALUES (@Username, @Email, @PasswordHash, @FirstName, @LastName, @PhoneNumber, @RoleId, @IsActive, @CreatedAt, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
    }

    protected override string GenerateUpdateQuery()
    {
        return @"
                UPDATE auth.[User] 
                SET Username = @Username, 
                    Email = @Email, 
                    PasswordHash = @PasswordHash, 
                    FirstName = @FirstName, 
                    LastName = @LastName, 
                    PhoneNumber = @PhoneNumber, 
                    RoleId = @RoleId, 
                    IsActive = @IsActive, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
    }
}