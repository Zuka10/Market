using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Auth;

namespace Market.Application.Features.Auth.Commands.LoginUser;

public record LoginUserCommand(string UsernameOrEmail, string Password, bool RememberMe) : ICommand<AuthResponse>
{
}