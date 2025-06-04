using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Auth;

namespace Market.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    long UserId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    long RoleId,
    bool IsActive
) : ICommand<bool>;