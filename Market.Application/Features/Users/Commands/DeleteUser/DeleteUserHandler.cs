using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteUserCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.Users.DeleteAsync(request.UserId);

        return BaseResponse<bool>.Success(true, "User deleted successfully.");
    }
}