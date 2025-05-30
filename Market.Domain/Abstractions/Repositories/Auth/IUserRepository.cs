using Market.Domain.Entities.Auth;
using Market.Domain.Filters;

namespace Market.Domain.Abstractions.Repositories.Auth;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetUserWithRoleAsync(long id);
    Task<IEnumerable<User>> GetUsersByRoleAsync(long roleId);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<bool> IsUsernameExistsAsync(string username);
    Task<bool> IsEmailExistsAsync(string email);
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);
    Task<PagedResult<User>> GetPagedUsersAsync(UserFilterParameters filterParams);
}