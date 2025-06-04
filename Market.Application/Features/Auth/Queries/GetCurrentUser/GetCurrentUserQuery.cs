using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Auth;

namespace Market.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery(long UserId) : IQuery<UserDto>
{
}