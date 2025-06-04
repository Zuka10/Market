using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Auth.Commands.RevokeTokens;

public class RevokeTokensHandler(IUnitOfWork unitOfWork, ITokenService tokenService)
    : ICommandHandler<RevokeTokensCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<BaseResponse<bool>> Handle(RevokeTokensCommand request, CancellationToken cancellationToken)
    {
        // Verify user exists and is active
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user is null || !user.IsActive)
        {
            return BaseResponse<bool>.Failure(["User not found or inactive."]);
        }

        // Revoke all refresh tokens for the user
        await _tokenService.RevokeAllUserTokensAsync(request.UserId);

        return BaseResponse<bool>.Success(true, "All tokens revoked successfully.");
    }
}