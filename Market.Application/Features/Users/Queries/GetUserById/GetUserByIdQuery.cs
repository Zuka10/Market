using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Auth;

namespace Market.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(long UserId) : IQuery<UserDto>
{
}