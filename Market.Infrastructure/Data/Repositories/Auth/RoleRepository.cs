using Dapper;
using Market.Domain.Abstractions.Repositories.Auth;
using Market.Domain.Entities.Auth;
using Market.Infrastructure.Constants;

namespace Market.Infrastructure.Data.Repositories.Auth;

public class RoleRepository(IDbConnectionFactory connectionFactory) :
    GenericRepository<Role>(connectionFactory, DatabaseConstants.Tables.Auth.Role, DatabaseConstants.Schemas.Auth),
    IRoleRepository
{
    public async Task<Role?> GetByNameAsync(string name)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE Name = @Name";
        var role = await connection.QueryFirstOrDefaultAsync<Role>(sql, new { Name = name });

        return role ?? throw new KeyNotFoundException($"Role with name '{name}' was not found.");
    }

    public async Task<IEnumerable<Role>> GetRolesWithUsersAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT r.*, u.Id, u.Username, u.Email, u.FirstName, u.LastName, u.IsActive
                FROM auth.Role r
                LEFT JOIN auth.[User] u ON r.Id = u.RoleId
                ORDER BY r.Name";

        var roleDict = new Dictionary<long, Role>();

        return await connection.QueryAsync<Role, User, Role>(
            sql,
            (role, user) =>
            {
                if (!roleDict.TryGetValue(role.Id, out var existingRole))
                {
                    existingRole = role;
                    existingRole.Users = [];
                    roleDict.Add(role.Id, existingRole);
                }

                if (user != null)
                {
                    existingRole.Users.Add(user);
                }

                return existingRole;
            },
            splitOn: "Id");
    }

    public async Task<int> GetUserCountByRoleAsync(long roleId)
    {
        if (!await ExistsAsync(roleId))
        {
            throw new KeyNotFoundException($"Role with ID '{roleId}' was not found.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM auth.[User] WHERE RoleId = @RoleId";
        return await connection.QuerySingleAsync<int>(sql, new { RoleId = roleId });
    }

    public override async Task UpdateAsync(Role entity)
    {
        if (!await ExistsAsync(entity.Id))
        {
            throw new KeyNotFoundException($"Role with ID '{entity.Id}' was not found and cannot be updated.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var nameCheckSql = $"SELECT COUNT(1) FROM {FullTableName} WHERE Name = @Name AND Id != @Id";
        var nameExists = await connection.QuerySingleAsync<int>(nameCheckSql, new { entity.Name, entity.Id });
        if (nameExists > 0)
        {
            throw new ArgumentException($"Role name '{entity.Name}' is already taken by another role.");
        }

        await base.UpdateAsync(entity);
    }

    public override async Task<Role> AddAsync(Role entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var nameCheckSql = $"SELECT COUNT(1) FROM {FullTableName} WHERE Name = @Name";
        var nameExists = await connection.QuerySingleAsync<int>(nameCheckSql, new { entity.Name });
        return nameExists > 0 ? throw new ArgumentException($"Role name '{entity.Name}' is already taken.") : await base.AddAsync(entity);
    }

    public override async Task DeleteAsync(long id)
    {
        if (!await ExistsAsync(id))
        {
            throw new KeyNotFoundException($"Role with ID '{id}' was not found and cannot be deleted.");
        }

        var userCount = await GetUserCountByRoleAsync(id);
        if (userCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete role with ID '{id}' because it has {userCount} associated user(s). Remove all users from this role first.");
        }

        await base.DeleteAsync(id);
    }

    protected override string GenerateInsertQuery()
    {
        return @"
                INSERT INTO auth.Role (Name)
                VALUES (@Name);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
    }

    protected override string GenerateUpdateQuery()
    {
        return @"
                UPDATE auth.Role 
                SET Name = @Name
                WHERE Id = @Id";
    }
}