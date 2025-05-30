using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateUserCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Get existing user
        var existingUser = await _unitOfWork.Users.GetUserWithRoleAsync(request.UserId);
        if (existingUser is null)
        {
            return BaseResponse<bool>.Failure(["User not found."]);
        }

        // Check for existing username (exclude current user)
        var existingUserByUsername = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
        if (existingUserByUsername is not null && existingUserByUsername.Id != request.UserId)
        {
            return BaseResponse<bool>.Failure([$"Username '{request.Username}' is already taken."]);
        }

        // Check for existing email (exclude current user)
        var existingUserByEmail = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (existingUserByEmail is not null && existingUserByEmail.Id != request.UserId)
        {
            return BaseResponse<bool>.Failure([$"Email '{request.Email}' is already registered."]);
        }

        // Validate role exists
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
        if (role is null)
        {
            return BaseResponse<bool>.Failure(["Invalid role specified."]);
        }

        // Update user properties
        existingUser.Username = request.Username.Trim();
        existingUser.Email = request.Email.Trim().ToLower();
        existingUser.FirstName = request.FirstName.Trim();
        existingUser.LastName = request.LastName.Trim();
        existingUser.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        existingUser.RoleId = request.RoleId;
        existingUser.IsActive = request.IsActive;
        existingUser.UpdatedAt = DateTime.UtcNow;

        // Update in database
        await _unitOfWork.Users.UpdateAsync(existingUser);
        return BaseResponse<bool>.Success(true, "User updated successfully.");
    }
}