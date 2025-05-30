using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Auth;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Auth.Commands.LoginUser;

public class LoginUserHandler(IUnitOfWork unitOfWork, ITokenService tokenService) : ICommandHandler<LoginUserCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<BaseResponse<AuthResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        // Find user by username or email
        var user = await _unitOfWork.Users.GetByUsernameAsync(request.UsernameOrEmail) ??
                  await _unitOfWork.Users.GetByEmailAsync(request.UsernameOrEmail);

        if (user is null)
        {
            return BaseResponse<AuthResponse>.Failure(["Invalid credentials."]);
        }

        if (!user.IsActive)
        {
            return BaseResponse<AuthResponse>.Failure(["Account is deactivated."]);
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return BaseResponse<AuthResponse>.Failure(["Invalid credentials."]);
        }

        // Get user with role
        var userWithRole = await _unitOfWork.Users.GetUserWithRoleAsync(user.Id);
        if (userWithRole is null)
        {
            return BaseResponse<AuthResponse>.Failure(["User role not found."]);
        }

        // Generate tokens
        var authResponse = await _tokenService.GenerateTokensAsync(userWithRole, request.RememberMe);

        return BaseResponse<AuthResponse>.Success(authResponse, "Login successful.");
    }
}