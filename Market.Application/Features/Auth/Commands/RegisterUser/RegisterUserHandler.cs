using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Auth;
using Market.Domain.Abstractions;
using Market.Domain.Entities.Auth;

namespace Market.Application.Features.Auth.Commands.RegisterUser;

public class RegisterUserHandler(IUnitOfWork unitOfWork, ITokenService tokenService) : ICommandHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<BaseResponse<AuthResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check for existing username
        var existingUserByUsername = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
        if (existingUserByUsername is not null)
        {
            return BaseResponse<AuthResponse>.Failure([$"Username '{request.Username}' is already taken."]);
        }

        // Check for existing email
        var existingUserByEmail = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (existingUserByEmail is not null)
        {
            return BaseResponse<AuthResponse>.Failure([$"Email '{request.Email}' is already registered."]);
        }

        // Validate role exists
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
        if (role is null)
        {
            return BaseResponse<AuthResponse>.Failure(["Invalid role specified."]);
        }

        // Create user
        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            RoleId = request.RoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdUser = await _unitOfWork.Users.AddAsync(user);
        createdUser.Role = role; // Set for response

        // Generate tokens
        var authResponse = await _tokenService.GenerateTokensAsync(createdUser);

        return BaseResponse<AuthResponse>.Success(authResponse, "User registered successfully.");
    }
}