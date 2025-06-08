using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Auth;

namespace Market.Application.Features.Roles.Queries.GetRoles;

public record GetRolesQuery : IQuery<List<RoleDto>>;