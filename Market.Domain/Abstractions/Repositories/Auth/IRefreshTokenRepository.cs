using Market.Domain.Entities.Auth;

namespace Market.Domain.Abstractions.Repositories.Auth;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserAsync(long userId);
    Task RevokeTokenAsync(string token);
    Task RevokeUserTokensAsync(long userId);
    Task CleanupExpiredTokensAsync();
}