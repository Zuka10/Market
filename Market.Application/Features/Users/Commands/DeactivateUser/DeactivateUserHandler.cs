using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Users.Commands.DeactivateUser;

public class DeactivateUserHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeactivateUserCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        // Get user with role information
        var user = await _unitOfWork.Users.GetUserWithRoleAsync(request.UserId);
        if (user is null)
        {
            return BaseResponse<bool>.Failure(["User not found."]);
        }

        // Check if user is already inactive
        if (!user.IsActive)
        {
            return BaseResponse<bool>.Failure(["User is already inactive."]);
        }

        // Deactivate user
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);

        return BaseResponse<bool>.Success(true, "User deactivated successfully.");
    }
}