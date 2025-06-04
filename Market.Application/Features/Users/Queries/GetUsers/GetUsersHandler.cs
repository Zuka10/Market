using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Auth;
using Market.Application.Features.Users.Queries.GetUsers;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Users.Queries.GetAllUsers;

public class GetUsersHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<PagedResult<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
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

        var pagedUsers = await _unitOfWork.Users.GetUsersAsync(filterParams);
        var userDtos = _mapper.Map<List<UserDto>>(pagedUsers.Items);

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

        var message = BuildSuccessMessage(request, pagedResult.TotalCount);
        return BaseResponse<PagedResult<UserDto>>.Success(pagedResult, message);
    }

    private static string BuildSuccessMessage(GetUsersQuery request, int totalCount)
    {
        if (HasAnyFilterCriteria(request))
        {
            return $"Retrieved {totalCount} vendors matching the filter criteria.";
        }

        return $"Retrieved {totalCount} vendors successfully.";
    }

    private static bool HasAnyFilterCriteria(GetUsersQuery request)
    {
        return !string.IsNullOrWhiteSpace(request.SearchTerm) ||
               !string.IsNullOrWhiteSpace(request.SortDirection) ||
               !string.IsNullOrWhiteSpace(request.SortBy) ||
               request.IsActive.HasValue ||
               request.RoleId.HasValue;
    }
}