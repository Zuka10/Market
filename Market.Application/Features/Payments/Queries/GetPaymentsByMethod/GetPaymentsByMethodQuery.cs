using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;
using Market.Domain.Enums;

namespace Market.Application.Features.Payments.Queries.GetPaymentsByMethod;

public record GetPaymentsByMethodQuery(PaymentMethod PaymentMethod) : IQuery<List<PaymentDto>>;