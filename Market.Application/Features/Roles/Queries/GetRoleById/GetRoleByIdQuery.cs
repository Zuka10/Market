using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Auth;

namespace Market.Application.Features.Roles.Queries.GetRoleById;

public record GetRoleByIdQuery(long RoleId) : IQuery<RoleDto>;