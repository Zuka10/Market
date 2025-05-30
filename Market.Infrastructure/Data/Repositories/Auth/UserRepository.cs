using Dapper;
using Market.Domain.Abstractions.Repositories.Auth;
using Market.Domain.Entities.Auth;
using Market.Domain.Filters;
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
        var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });

        return user ?? throw new KeyNotFoundException($"User with username '{username}' not found in {FullTableName}.");
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE Email = @Email";
        var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });

        return user ?? throw new KeyNotFoundException($"User with email '{email}' was not found.");
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

        return result.FirstOrDefault() ?? throw new KeyNotFoundException($"User with ID '{id}' was not found.");
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

    public async Task<PagedResult<User>> GetPagedUsersAsync(UserFilterParameters filterParams)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        // Build WHERE clause based on filters
        if (filterParams.IsActive.HasValue)
        {
            whereConditions.Add("u.IsActive = @IsActive");
            parameters.Add("IsActive", filterParams.IsActive.Value);
        }

        if (!string.IsNullOrEmpty(filterParams.SearchTerm))
        {
            whereConditions.Add("(u.Username LIKE @SearchTerm OR u.FirstName LIKE @SearchTerm OR u.LastName LIKE @SearchTerm OR u.Email LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{filterParams.SearchTerm}%");
        }

        if (filterParams.RoleId.HasValue)
        {
            whereConditions.Add("u.RoleId = @RoleId");
            parameters.Add("RoleId", filterParams.RoleId.Value);
        }

        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";

        // Build ORDER BY clause
        var validSortFields = new Dictionary<string, string>
        {
            { "id", "u.Id" },
            { "username", "u.Username" },
            { "email", "u.Email" },
            { "firstname", "u.FirstName" },
            { "lastname", "u.LastName" },
            { "createdat", "u.CreatedAt" },
            { "updatedat", "u.UpdatedAt" }
        };

        var sortField = validSortFields.ContainsKey(filterParams.SortBy?.ToLower() ?? "id")
            ? validSortFields[filterParams.SortBy.ToLower()]
            : validSortFields["id"];

        var sortDirection = filterParams.SortDirection?.ToLower() == "desc" ? "DESC" : "ASC";
        var orderByClause = $"ORDER BY {sortField} {sortDirection}";

        // Calculate pagination
        var offset = (filterParams.PageNumber - 1) * filterParams.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filterParams.PageSize);

        // Count query
        var countSql = $@"
            SELECT COUNT(*)
            FROM {FullTableName} u
            LEFT JOIN auth.Role r ON u.RoleId = r.Id
            {whereClause}";

        // Data query with role information
        var dataSql = $@"
            SELECT u.*, r.Id as RoleId_Split, r.Name as RoleName
            FROM {FullTableName} u
            LEFT JOIN auth.Role r ON u.RoleId = r.Id
            {whereClause}
            {orderByClause}
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        // Execute queries
        var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

        var users = await connection.QueryAsync<User, Role, User>(
            dataSql,
            (user, role) =>
            {
                user.Role = role;
                return user;
            },
            parameters,
            splitOn: "RoleId_Split"
        );

        // Calculate pagination metadata
        var totalPages = (int)Math.Ceiling((double)totalCount / filterParams.PageSize);

        return new PagedResult<User>
        {
            Items = users.ToList(),
            TotalCount = totalCount,
            Page = filterParams.PageNumber,
            PageSize = filterParams.PageSize,
            TotalPages = totalPages,
            HasNextPage = filterParams.PageNumber < totalPages,
            HasPreviousPage = filterParams.PageNumber > 1
        };
    }

    public override async Task UpdateAsync(User entity)
    {
        if (!await ExistsAsync(entity.Id))
        {
            throw new KeyNotFoundException($"User with ID '{entity.Id}' was not found and cannot be updated.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var usernameCheckSql = $"SELECT COUNT(1) FROM {FullTableName} WHERE Username = @Username AND Id != @Id";
        var usernameExists = await connection.QuerySingleAsync<int>(usernameCheckSql, new { Username = entity.Username, Id = entity.Id });
        if (usernameExists > 0)
        {
            throw new ArgumentException($"Username '{entity.Username}' is already taken by another user.");
        }

        var emailCheckSql = $"SELECT COUNT(1) FROM {FullTableName} WHERE Email = @Email AND Id != @Id";
        var emailExists = await connection.QuerySingleAsync<int>(emailCheckSql, new { Email = entity.Email, Id = entity.Id });
        if (emailExists > 0)
        {
            throw new ArgumentException($"Email '{entity.Email}' is already taken by another user.");
        }

        await base.UpdateAsync(entity);
    }

    public override async Task<User> AddAsync(User entity)
    {
        if (await IsUsernameExistsAsync(entity.Username))
        {
            throw new ArgumentException($"Username '{entity.Username}' is already taken.");
        }

        if (await IsEmailExistsAsync(entity.Email))
        {
            throw new ArgumentException($"Email '{entity.Email}' is already taken.");
        }

        return await base.AddAsync(entity);
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