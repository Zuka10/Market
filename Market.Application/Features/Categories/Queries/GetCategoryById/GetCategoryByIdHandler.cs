using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        // Retrieve the category by ID, including its products
        var category = await _unitOfWork.Categories.GetCategoryWithProductsAsync(request.Id);
        if (category is null)
        {
            return BaseResponse<CategoryDto>.Failure(["Category not found."]);
        }

        var categoryDto = _mapper.Map<CategoryDto>(category);
        return BaseResponse<CategoryDto>.Success(categoryDto, "Category retrieved successfully.");
    }
}