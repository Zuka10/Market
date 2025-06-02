using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Enums;
using Market.Domain.Filters;

namespace Market.Application.Features.Payments.Queries.GetPayments;

public class GetPaymentsHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetPaymentsQuery, PagedResult<PaymentDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<PagedResult<PaymentDto>>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var filterParams = new PaymentFilterParameters
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm,
            PaymentMethod = ParsePaymentMethod(request.PaymentMethod),
            Status = ParsePaymentStatus(request.Status),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            SortBy = request.SortBy ?? "PaymentDate",
            SortDirection = request.SortDirection ?? "DESC",
            OrderId = request.OrderId,
            OrderNumber = request.OrderNumber,
            MinAmount = request.MinAmount,
            MaxAmount = request.MaxAmount,
            UserId = request.UserId,
            LocationId = request.LocationId,
            CustomerName = request.CustomerName
        };

        var pagedPayments = await _unitOfWork.Payments.GetPaymentsAsync(filterParams);
        var paymentDtos = _mapper.Map<List<PaymentDto>>(pagedPayments.Items);

        var result = new PagedResult<PaymentDto>
        {
            Items = paymentDtos,
            TotalCount = pagedPayments.TotalCount,
            Page = pagedPayments.Page,
            PageSize = pagedPayments.PageSize,
            TotalPages = pagedPayments.TotalPages,
            HasNextPage = pagedPayments.HasNextPage,
            HasPreviousPage = pagedPayments.HasPreviousPage
        };

        return BaseResponse<PagedResult<PaymentDto>>.Success(result, $"Retrieved {paymentDtos.Count} payments.");
    }

    private static PaymentMethod? ParsePaymentMethod(string? paymentMethod)
    {
        if (string.IsNullOrEmpty(paymentMethod))
        {
            return null;
        }

        return Enum.TryParse<PaymentMethod>(paymentMethod, true, out var result) ? result : null;
    }

    private static PaymentStatus? ParsePaymentStatus(string? status)
    {
        if (string.IsNullOrEmpty(status))
        {
            return null;
        }

        return Enum.TryParse<PaymentStatus>(status, true, out var result) ? result : null;
    }
}