using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Auth;
using Market.Domain.Abstractions;
using Market.Domain.Entities.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Market.Application.Services.Token;

public class TokenService : ITokenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public TokenService(IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtOptions)
    {
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResponse> GenerateTokensAsync(User user, bool rememberMe = false)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, rememberMe);

        return new AuthResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName!,
            LastName = user.LastName!,
            FullName = user.FullName,
            Role = user.Role?.Name ?? string.Empty,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
        if (token == null || !token.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var user = await _unitOfWork.Users.GetUserWithRoleAsync(token.UserId);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive.");
        }

        // Mark old token as used
        token.IsUsed = true;
        await _unitOfWork.RefreshTokens.UpdateAsync(token);

        // Generate new tokens
        return await GenerateTokensAsync(user);
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        await _unitOfWork.RefreshTokens.RevokeTokenAsync(refreshToken);
        return true;
    }

    public async Task<bool> RevokeAllUserTokensAsync(long userId)
    {
        await _unitOfWork.RefreshTokens.RevokeUserTokensAsync(userId);
        return true;
    }

    private string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.GivenName, user.FirstName!),
                new(ClaimTypes.Surname, user.LastName!),
                new(ClaimTypes.Role, user.Role?.Name ?? string.Empty),
                new("user_id", user.Id.ToString()),
                new("role_id", user.RoleId.ToString()),
                new("full_name", user.FullName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(long userId, bool rememberMe = false)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = GenerateRandomToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(rememberMe ? 30 : _jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            IsUsed = false
        };

        return await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
    }
    public string GeneratePasswordResetToken(long userId, string email, TimeSpan expiration)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim("userId", userId.ToString()),
            new Claim("email", email),
            new Claim("tokenType", "password-reset"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        }),
            Expires = DateTime.UtcNow.Add(expiration),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public TokenValidationResult ValidatePasswordResetToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // No tolerance for expiration
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // Verify it's a password reset token
            var tokenTypeClaim = principal.FindFirst("tokenType");
            if (tokenTypeClaim?.Value != "password-reset")
            {
                return new TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid token type."
                };
            }

            // Extract claims
            var userIdClaim = principal.FindFirst("userId");
            var emailClaim = principal.FindFirst("email");

            if (userIdClaim == null || emailClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                return new TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid token claims."
                };
            }

            return new TokenValidationResult
            {
                IsValid = true,
                UserId = userId,
                Email = emailClaim.Value
            };
        }
        catch (SecurityTokenExpiredException)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token has expired."
            };
        }
        catch (SecurityTokenException)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid token."
            };
        }
        catch (Exception)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token validation failed."
            };
        }
    }

    private static string GenerateRandomToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}