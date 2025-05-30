using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Auth;
using Market.Domain.Abstractions;
using Market.Domain.Entities.Auth;

namespace Market.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleHandler(IUnitOfWork unitOfWork, IMapper mapper) : ICommandHandler<CreateRoleCommand, RoleDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // Check if role name already exists
        var existingRole = await _unitOfWork.Roles.GetByNameAsync(request.Name);
        if (existingRole != null)
        {
            return BaseResponse<RoleDto>.Failure([$"Role name '{request.Name}' already exists."]);
        }

        // Create new role
        var role = new Role
        {
            Name = request.Name.Trim()
        };

        var createdRole = await _unitOfWork.Roles.AddAsync(role);
        var roleDto = _mapper.Map<RoleDto>(createdRole);

        return BaseResponse<RoleDto>.Success(roleDto, "Role created successfully.");
    }
}