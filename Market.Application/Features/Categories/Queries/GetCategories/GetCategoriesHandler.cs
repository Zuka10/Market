using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        // Retrieve all categories with their products
        var categories = await _unitOfWork.Categories.GetCategoriesWithProductsAsync();
        if (categories is null || !categories.Any())
        {
            return BaseResponse<List<CategoryDto>>.Failure(["No categories found."]);
        }

        var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
        return BaseResponse<List<CategoryDto>>.Success(categoryDtos, "Categories retrieved successfully.");
    }
}