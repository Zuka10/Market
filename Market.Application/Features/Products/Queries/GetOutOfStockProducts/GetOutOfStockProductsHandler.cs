using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Products.Queries.GetOutOfStockProducts;

public class GetOutOfStockProductsHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetOutOfStockProductsQuery, List<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<ProductDto>>> Handle(GetOutOfStockProductsQuery request, CancellationToken cancellationToken)
    {
        var filterParams = new ProductFilterParameters
        {
            IsOutOfStock = true,
            LocationId = request.LocationId,
            CategoryId = request.CategoryId,
            IsAvailable = true,
            SortBy = "updatedat",
            SortDirection = "desc",
            PageSize = int.MaxValue
        };

        var products = await _unitOfWork.Products.GetProductsAsync(filterParams);
        var productDtos = _mapper.Map<List<ProductDto>>(products.Items);

        return BaseResponse<List<ProductDto>>.Success(productDtos, $"Found {productDtos.Count} out of stock products.");
    }
}