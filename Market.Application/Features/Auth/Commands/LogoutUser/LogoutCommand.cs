using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Auth.Commands.LogoutUser;

public record LogoutCommand(string RefreshToken) : ICommand<bool>
{
}