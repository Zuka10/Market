using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(long UserId) : ICommand<bool>
{ 
}