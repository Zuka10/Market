using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(string Token, string NewPassword, string ConfirmPassword) : ICommand<bool>
{
}