using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Categories.Queries.SearchCategories;

public class SearchCategoriesHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<SearchCategoriesQuery, List<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<CategoryDto>>> Handle(SearchCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Categories.SearchCategoriesAsync(request.SearchTerm);
        if (categories is null || !categories.Any())
        {
            return BaseResponse<List<CategoryDto>>.Failure(["No categories found matching the search term."]);
        }

        var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
        return BaseResponse<List<CategoryDto>>.Success(categoryDtos, "Categories retrieved successfully.");
    }
}