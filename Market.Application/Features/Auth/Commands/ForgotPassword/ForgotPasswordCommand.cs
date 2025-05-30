using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : ICommand<bool>
{
}