using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteRoleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        // Get role
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
        if (role == null)
        {
            return BaseResponse<bool>.Failure(["Role not found."]);
        }

        // Check if role has users assigned
        var usersInRole = await _unitOfWork.Users.GetUsersByRoleAsync(request.RoleId);
        if (usersInRole.Any())
        {
            return BaseResponse<bool>.Failure([$"Cannot delete role '{role.Name}' because it has {usersInRole.Count()} users assigned."]);
        }

        // Delete role
        await _unitOfWork.Roles.DeleteAsync(request.RoleId);

        return BaseResponse<bool>.Success(true, $"Role '{role.Name}' deleted successfully.");
    }
}