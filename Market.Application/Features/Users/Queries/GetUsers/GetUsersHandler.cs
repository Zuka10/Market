using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Auth;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Users.Queries.GetAllUsers;

public class GetUsersHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<PagedResult<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // Create filter parameters
        var filterParams = new UserFilterParameters
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm?.Trim(),
            RoleId = request.RoleId,
            IsActive = request.IsActive,
            SortBy = request.SortBy?.Trim(),
            SortDirection = request.SortDirection?.Trim()?.ToLower()
        };

        // Get paginated and filtered users
        var pagedUsers = await _unitOfWork.Users.GetPagedUsersAsync(filterParams);

        // Map to DTOs using existing UserDto
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

        return BaseResponse<PagedResult<UserDto>>.Success(pagedResult, "Users retrieved successfully.");
    }
}