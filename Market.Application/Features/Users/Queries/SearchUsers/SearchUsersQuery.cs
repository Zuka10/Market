using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Auth;

namespace Market.Application.Features.Users.Queries.SearchUsers;

public record SearchUsersQuery(string SearchTerm) : IQuery<List<UserDto>>;