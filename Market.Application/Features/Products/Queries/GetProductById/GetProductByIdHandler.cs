using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetProductWithDetailsAsync(request.Id);
        if (product is null)
        {
            return BaseResponse<ProductDto>.Failure(["Product not found."]);
        }

        var productDto = _mapper.Map<ProductDto>(product);
        return BaseResponse<ProductDto>.Success(productDto, "Product retrieved successfully.");
    }
}