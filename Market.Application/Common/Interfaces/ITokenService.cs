using Market.Application.DTOs.Auth;
using Market.Domain.Entities.Auth;
using Market.Application.Services.Token;

namespace Market.Application.Common.Interfaces;

public interface ITokenService
{
    Task<AuthResponse> GenerateTokensAsync(User user, bool rememberMe = false);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<bool> RevokeAllUserTokensAsync(long userId);
    string GeneratePasswordResetToken(long userId, string email, TimeSpan expiration);
    TokenValidationResult ValidatePasswordResetToken(string token);
}