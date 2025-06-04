using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(long UserId, string CurrentPassword, string NewPassword, string ConfirmPassword)
    : ICommand<bool>
{
}