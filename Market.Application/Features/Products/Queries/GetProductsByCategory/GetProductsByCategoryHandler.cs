using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Products.Queries.GetProductsByCategory;

public class GetProductsByCategoryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetProductsByCategoryQuery, List<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<ProductDto>>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetProductsByCategoryAsync(request.CategoryId);

        if (request.IsAvaliable.HasValue)
        {
            products = products.Where(p => p.IsAvailable == request.IsAvaliable.Value);
        }

        var productDtos = _mapper.Map<List<ProductDto>>(products);
        return BaseResponse<List<ProductDto>>.Success(productDtos, $"Found {productDtos.Count} products in category.");
    }
}