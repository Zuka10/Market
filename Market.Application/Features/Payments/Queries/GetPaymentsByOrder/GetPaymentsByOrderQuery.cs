using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Payments.Queries.GetPaymentsByOrder;

public record GetPaymentsByOrderQuery(long OrderId) : IQuery<List<PaymentDto>>;