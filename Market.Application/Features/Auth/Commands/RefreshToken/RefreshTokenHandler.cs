using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Auth;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenHandler(IUnitOfWork unitOfWork, ITokenService tokenService)
    : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<BaseResponse<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BaseResponse<AuthResponse>.Failure(["Refresh token is required."]);
        }

        // Validate refresh token
        var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);
        if (refreshToken is null)
        {
            return BaseResponse<AuthResponse>.Failure(["Invalid refresh token."]);
        }

        if (refreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            return BaseResponse<AuthResponse>.Failure(["Refresh token has expired."]);
        }

        if (refreshToken.IsRevoked)
        {
            return BaseResponse<AuthResponse>.Failure(["Refresh token has been revoked."]);
        }

        // Get user with role
        var user = await _unitOfWork.Users.GetUserWithRoleAsync(refreshToken.UserId);
        if (user is null || !user.IsActive)
        {
            return BaseResponse<AuthResponse>.Failure(["User not found or inactive."]);
        }

        // Revoke the old refresh token
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.RefreshTokens.UpdateAsync(refreshToken);

        // Generate new tokens
        var authResponse = await _tokenService.GenerateTokensAsync(user);

        return BaseResponse<AuthResponse>.Success(authResponse, "Tokens refreshed successfully.");
    }
}