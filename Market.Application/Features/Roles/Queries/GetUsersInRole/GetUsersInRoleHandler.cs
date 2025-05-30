using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Auth;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Roles.Queries.GetUsersInRole;

public class GetUsersInRoleHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetUsersInRoleQuery, RoleDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<RoleDto>> Handle(GetUsersInRoleQuery request, CancellationToken cancellationToken)
    {
        // Get role
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
        if (role is null)
        {
            return BaseResponse<RoleDto>.Failure(["Role not found."]);
        }

        // Get users in role
        var users = await _unitOfWork.Users.GetUsersByRoleAsync(request.RoleId);
        var userDtos = _mapper.Map<List<UserDto>>(users);

        // Create role DTO with users
        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            UserCount = userDtos.Count,
            Users = userDtos
        };

        return BaseResponse<RoleDto>.Success(roleDto, $"Role '{role.Name}' with users retrieved successfully.");
    }
}