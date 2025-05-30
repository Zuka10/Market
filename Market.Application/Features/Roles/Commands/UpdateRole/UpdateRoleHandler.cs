using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateRoleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        // Get existing role
        var existingRole = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
        if (existingRole == null)
        {
            return BaseResponse<bool>.Failure(["Role not found."]);
        }

        // Check if role name already exists (exclude current role)
        var roleWithSameName = await _unitOfWork.Roles.GetByNameAsync(request.Name);
        if (roleWithSameName != null && roleWithSameName.Id != request.RoleId)
        {
            return BaseResponse<bool>.Failure([$"Role name '{request.Name}' already exists."]);
        }

        // Update role
        existingRole.Name = request.Name.Trim();

        await _unitOfWork.Roles.UpdateAsync(existingRole);

        return BaseResponse<bool>.Success(true, "Role updated successfully.");
    }
}