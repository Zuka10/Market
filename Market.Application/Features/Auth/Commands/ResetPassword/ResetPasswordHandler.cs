using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordHandler(IUnitOfWork unitOfWork, ITokenService tokenService)
    : ICommandHandler<ResetPasswordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<BaseResponse<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Validate and decode the reset token
        var tokenValidation = _tokenService.ValidatePasswordResetToken(request.Token);
        if (!tokenValidation.IsValid)
        {
            return BaseResponse<bool>.Failure(["Invalid or expired reset token."]);
        }

        // Get user
        var user = await _unitOfWork.Users.GetByIdAsync(tokenValidation.UserId);
        if (user is null || !user.IsActive)
        {
            return BaseResponse<bool>.Failure(["User not found or inactive."]);
        }

        // Verify email matches (additional security check)
        if (!string.Equals(user.Email, tokenValidation.Email, StringComparison.OrdinalIgnoreCase))
        {
            return BaseResponse<bool>.Failure(["Invalid reset token."]);
        }

        // Check if new password is different from current (optional security measure)
        if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
        {
            return BaseResponse<bool>.Failure(["New password must be different from current password."]);
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);

        // Revoke all refresh tokens to force re-login on all devices
        await _tokenService.RevokeAllUserTokensAsync(user.Id);

        return BaseResponse<bool>.Success(true, "Password reset successfully.");
    }
}