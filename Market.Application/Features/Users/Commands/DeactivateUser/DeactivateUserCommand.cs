using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Auth;

namespace Market.Application.Features.Users.Commands.DeactivateUser;

public record DeactivateUserCommand(long UserId) : ICommand<bool>;