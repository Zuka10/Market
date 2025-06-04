using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Products.Queries.GetProducts;

public class GetProductsHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var filterParams = new ProductFilterParameters
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm?.Trim(),
            IsAvailable = request.IsAvailable,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            MinStock = request.MinStock,
            MaxStock = request.MaxStock,
            CategoryId = request.CategoryId,
            LocationId = request.LocationId,
            Unit = request.Unit?.Trim(),
            SortBy = request.SortBy?.Trim(),
            SortDirection = request.SortDirection?.Trim()?.ToLower()
        };

        var pagedProducts = await _unitOfWork.Products.GetProductsAsync(filterParams);
        var productDtos = _mapper.Map<List<ProductDto>>(pagedProducts.Items);

        var pagedResult = new PagedResult<ProductDto>
        {
            Items = productDtos,
            TotalCount = pagedProducts.TotalCount,
            Page = pagedProducts.Page,
            PageSize = pagedProducts.PageSize,
            TotalPages = pagedProducts.TotalPages,
            HasNextPage = pagedProducts.HasNextPage,
            HasPreviousPage = pagedProducts.HasPreviousPage
        };

        return BaseResponse<PagedResult<ProductDto>>.Success(pagedResult, $"Retrieved {pagedResult.TotalCount} products successfully.");
    }
}