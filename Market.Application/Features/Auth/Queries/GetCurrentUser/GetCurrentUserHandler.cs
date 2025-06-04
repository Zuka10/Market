using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Auth;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        // Get user with role information
        var user = await _unitOfWork.Users.GetUserWithRoleAsync(request.UserId);
        if (user is null || !user.IsActive)
        {
            return BaseResponse<UserDto>.Failure(["User not found or inactive."]);
        }

        // Map entity to DTO using AutoMapper
        var userDto = _mapper.Map<UserDto>(user);

        return BaseResponse<UserDto>.Success(userDto, "User information retrieved successfully.");
    }
}