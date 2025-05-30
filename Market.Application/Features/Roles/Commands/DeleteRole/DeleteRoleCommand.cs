using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Roles.Commands.DeleteRole;

public record DeleteRoleCommand(long RoleId) : ICommand<bool>
{
}