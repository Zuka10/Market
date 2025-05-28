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
        return await connection.QueryFirstOrDefaultAsync<Role>(sql, new { Name = name });
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
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM auth.[User] WHERE RoleId = @RoleId";
        return await connection.QuerySingleAsync<int>(sql, new { RoleId = roleId });
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