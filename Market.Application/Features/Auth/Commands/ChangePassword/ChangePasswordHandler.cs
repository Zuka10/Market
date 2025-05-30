using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordHandler(IUnitOfWork unitOfWork) : ICommandHandler<ChangePasswordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // Get user
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null || !user.IsActive)
        {
            return BaseResponse<bool>.Failure(["User not found or inactive."]);
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BaseResponse<bool>.Failure(["Current password is incorrect."]);
        }

        // Check if new password is different from current
        if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
        {
            return BaseResponse<bool>.Failure(["New password must be different from current password."]);
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);

        await _unitOfWork.RefreshTokens.RevokeUserTokensAsync(user.Id);

        return BaseResponse<bool>.Success(true, "Password changed successfully.");
    }
}