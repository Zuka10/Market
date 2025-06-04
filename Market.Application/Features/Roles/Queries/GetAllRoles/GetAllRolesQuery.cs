using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Auth;

namespace Market.Application.Features.Roles.Queries.GetAllRoles;

public record GetAllRolesQuery : IQuery<List<RoleDto>>;