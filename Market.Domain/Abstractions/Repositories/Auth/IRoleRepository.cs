using Market.Domain.Entities.Auth;

namespace Market.Domain.Abstractions.Repositories.Auth;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetRolesWithUsersAsync();
    Task<int> GetUserCountByRoleAsync(long roleId);
}