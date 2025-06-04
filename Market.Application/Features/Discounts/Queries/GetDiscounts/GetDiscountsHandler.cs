using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Discounts.Queries.GetDiscounts;

public class GetAllDiscountsHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetDiscountsQuery, PagedResult<DiscountDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<PagedResult<DiscountDto>>> Handle(GetDiscountsQuery request, CancellationToken cancellationToken)
    {
        var filterParams = new DiscountFilterParameters
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm?.Trim(),
            IsActive = request.IsActive,
            IsValid = request.IsValid,
            MinPercentage = request.MinPercentage,
            MaxPercentage = request.MaxPercentage,
            LocationId = request.LocationId,
            VendorId = request.VendorId,
            StartDateFrom = request.StartDateFrom,
            StartDateTo = request.StartDateTo,
            EndDateFrom = request.EndDateFrom,
            EndDateTo = request.EndDateTo,
            SortBy = request.SortBy?.Trim(),
            SortDirection = request.SortDirection?.Trim()?.ToLower()
        };

        var pagedDiscounts = await _unitOfWork.Discounts.GetDiscountsAsync(filterParams);
        var discountDtos = _mapper.Map<List<DiscountDto>>(pagedDiscounts.Items);

        var pagedResult = new PagedResult<DiscountDto>
        {
            Items = discountDtos,
            TotalCount = pagedDiscounts.TotalCount,
            Page = pagedDiscounts.Page,
            PageSize = pagedDiscounts.PageSize,
            TotalPages = pagedDiscounts.TotalPages,
            HasNextPage = pagedDiscounts.HasNextPage,
            HasPreviousPage = pagedDiscounts.HasPreviousPage
        };

        return BaseResponse<PagedResult<DiscountDto>>.Success(pagedResult, $"Retrieved {pagedResult.TotalCount} discounts successfully.");
    }
}