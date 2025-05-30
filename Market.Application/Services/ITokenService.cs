using Market.Application.DTOs.Auth;
using Market.Domain.Entities.Auth;

namespace Market.Application.Services;

public interface ITokenService
{
    Task<AuthResponse> GenerateTokensAsync(User user, bool rememberMe = false);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<bool> RevokeAllUserTokensAsync(long userId);
}