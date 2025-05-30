using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Auth.Commands.RevokeTokens;

public record RevokeTokensCommand(long UserId) : ICommand<bool>
{
}