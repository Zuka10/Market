using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Auth;
using Market.Domain.Filters;

namespace Market.Application.Features.Users.Queries.GetAllUsers;

public record GetUsersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    long? RoleId = null,
    bool? IsActive = null,
    string? SortBy = null,
    string? SortDirection = "asc"
) : IQuery<PagedResult<UserDto>>;