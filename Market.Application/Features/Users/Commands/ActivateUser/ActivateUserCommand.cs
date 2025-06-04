using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Users.Commands.ActivateUser;

public record ActivateUserCommand(long UserId) : ICommand<bool>;