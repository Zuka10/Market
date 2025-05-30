using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Auth;

namespace Market.Application.Features.Roles.Commands.CreateRole;

public record CreateRoleCommand(string Name) : ICommand<RoleDto>;