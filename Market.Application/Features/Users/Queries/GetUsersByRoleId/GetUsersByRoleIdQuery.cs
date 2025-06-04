using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Auth;
using Market.Domain.Filters;

namespace Market.Application.Features.Users.Queries.GetUsersByRole;

public record GetUsersByRoleQuery(
    long RoleId,
    int PageNumber = 1,
    int PageSize = 20,
    bool? IsActive = null,
    string? SearchTerm = null
) : IQuery<PagedResult<UserDto>>;