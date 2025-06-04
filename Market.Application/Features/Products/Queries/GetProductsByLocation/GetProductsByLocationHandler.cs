using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Products.Queries.GetProductsByLocation;

public class GetProductsByLocationHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetProductsByLocationQuery, List<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<ProductDto>>> Handle(GetProductsByLocationQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetProductsByLocationAsync(request.LocationId);

        if (request.IsAvailable.HasValue)
        {
            products = products.Where(p => p.IsAvailable == request.IsAvailable.Value);
        }

        var productDtos = _mapper.Map<List<ProductDto>>(products);
        return BaseResponse<List<ProductDto>>.Success(productDtos, $"Found {productDtos.Count} products in location.");
    }
}