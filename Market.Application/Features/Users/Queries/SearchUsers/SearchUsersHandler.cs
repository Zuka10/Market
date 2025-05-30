using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Auth;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Users.Queries.SearchUsers;

public class SearchUsersHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<SearchUsersQuery, List<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<UserDto>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.SearchUsersAsync(request.SearchTerm);

        var userDtos = _mapper.Map<List<UserDto>>(users);

        return BaseResponse<List<UserDto>>.Success(userDtos, $"Found {userDtos.Count} users matching '{request.SearchTerm}'.");
    }
}