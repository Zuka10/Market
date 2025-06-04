using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Entities.Market;

namespace Market.Application.Features.Products.Commands.CreateProduct;

public class CreateProductHandler(IUnitOfWork unitOfWork, IMapper mapper) : ICommandHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            InStock = request.InStock,
            Unit = request.Unit.Trim().ToLowerInvariant(),
            LocationId = request.LocationId,
            CategoryId = request.CategoryId,
            IsAvailable = request.IsAvailable,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdProduct = await _unitOfWork.Products.AddAsync(product);
        var productDto = _mapper.Map<ProductDto>(createdProduct);

        return BaseResponse<ProductDto>.Success(productDto, "Product created successfully.");
    }
}