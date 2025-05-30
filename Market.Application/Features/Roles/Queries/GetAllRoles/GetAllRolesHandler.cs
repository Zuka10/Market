using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Auth;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Roles.Queries.GetAllRoles;

public class GetAllRolesHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetAllRolesQuery, List<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<RoleDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.Roles.GetAllAsync();
        var roleDtos = _mapper.Map<List<RoleDto>>(roles);

        return BaseResponse<List<RoleDto>>.Success(roleDtos, "Roles retrieved successfully.");
    }
}