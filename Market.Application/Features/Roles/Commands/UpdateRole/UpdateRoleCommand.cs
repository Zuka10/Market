using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Roles.Commands.UpdateRole;

public record UpdateRoleCommand(long RoleId, string Name) : ICommand<bool>;