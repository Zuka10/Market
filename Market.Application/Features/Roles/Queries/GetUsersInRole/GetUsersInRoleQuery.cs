using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Auth;

namespace Market.Application.Features.Roles.Queries.GetUsersInRole;

public record GetUsersInRoleQuery(long RoleId) : IQuery<RoleDto>;