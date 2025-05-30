using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Auth;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // Get user by ID
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null || !user.IsActive)
        {
            return BaseResponse<UserDto>.Failure(["User not found or inactive."]);
        }

        // Map entity to DTO using AutoMapper
        var userDto = _mapper.Map<UserDto>(user);

        return BaseResponse<UserDto>.Success(userDto, "User retrieved successfully.");
    }
}