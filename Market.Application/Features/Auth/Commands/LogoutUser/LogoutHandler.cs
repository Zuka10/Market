using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Auth.Commands.LogoutUser;

public class LogoutHandler(IUnitOfWork unitOfWork) : ICommandHandler<LogoutCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BaseResponse<bool>.Failure(["Refresh token is required."]);
        }

        // Find and revoke the refresh token
        var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);
        if (refreshToken is null)
        {
            return BaseResponse<bool>.Failure(["Invalid refresh token."]);
        }

        if (refreshToken.IsRevoked)
        {
            return BaseResponse<bool>.Success(true, "User already logged out.");
        }

        // Revoke the refresh token
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.RefreshTokens.UpdateAsync(refreshToken);

        return BaseResponse<bool>.Success(true, "User logged out successfully.");
    }
}