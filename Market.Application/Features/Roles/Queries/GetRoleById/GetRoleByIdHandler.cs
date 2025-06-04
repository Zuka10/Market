using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Auth;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetRoleByIdQuery, RoleDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
        if (role is null)
        {
            return BaseResponse<RoleDto>.Failure(["Role not found."]);
        }

        var roleDto = _mapper.Map<RoleDto>(role);
        return BaseResponse<RoleDto>.Success(roleDto, "Role retrieved successfully.");
    }
}