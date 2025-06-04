using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Auth;

namespace Market.Application.Features.Auth.Commands.RegisterUser;

public record RegisterUserCommand(string Username, string Email, string Password,
    string ConfirmPassword, string FirstName, string LastName, string? PhoneNumber, long RoleId = 3) :
    ICommand<AuthResponse>
{
}