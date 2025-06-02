using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;
using Market.Domain.Filters;

namespace Market.Application.Features.Payments.Queries.GetPayments;

public record GetPaymentsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? PaymentMethod = null,
    string? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? SortBy = "PaymentDate",
    string? SortDirection = "DESC",
    long? OrderId = null,
    string? OrderNumber = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    long? UserId = null,
    long? LocationId = null,
    string? CustomerName = null
) : IQuery<PagedResult<PaymentDto>>;