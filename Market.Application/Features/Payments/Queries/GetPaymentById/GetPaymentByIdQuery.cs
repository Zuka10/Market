using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Payments.Queries.GetPaymentById;

public record GetPaymentByIdQuery(long Id) : IQuery<PaymentDto>;