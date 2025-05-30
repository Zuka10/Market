using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Auth;
using Market.Application.Features.Users.Queries.GetUsersByRole;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Users.Queries.GetUsersByRoleId;

public class GetUsersByRoleIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetUsersByRoleQuery, PagedResult<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<PagedResult<UserDto>>> Handle(GetUsersByRoleQuery request, CancellationToken cancellationToken)
    {
        // Validate role exists
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
        if (role is null)
        {
            return BaseResponse<PagedResult<UserDto>>.Failure(["Role not found."]);
        }

        // Create filter parameters
        var filterParams = new UserFilterParameters
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            RoleId = request.RoleId,
            IsActive = request.IsActive,
            SearchTerm = request.SearchTerm?.Trim(),
            SortBy = "Username",
            SortDirection = "asc"
        };

        // Get paginated users by role
        var pagedUsers = await _unitOfWork.Users.GetPagedUsersAsync(filterParams);

        // Map to DTOs
        var userDtos = _mapper.Map<List<UserDto>>(pagedUsers.Items);

        // Create paged result
        var pagedResult = new PagedResult<UserDto>
        {
            Items = userDtos,
            TotalCount = pagedUsers.TotalCount,
            Page = pagedUsers.Page,
            PageSize = pagedUsers.PageSize,
            TotalPages = pagedUsers.TotalPages,
            HasNextPage = pagedUsers.HasNextPage,
            HasPreviousPage = pagedUsers.HasPreviousPage
        };

        return BaseResponse<PagedResult<UserDto>>.Success(pagedResult, $"Users with role '{role.Name}' retrieved successfully.");
    }
}