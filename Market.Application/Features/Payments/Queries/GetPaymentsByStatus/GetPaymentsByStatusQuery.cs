using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;
using Market.Domain.Enums;

namespace Market.Application.Features.Payments.Queries.GetPaymentsByStatus;

public record GetPaymentsByStatusQuery(PaymentStatus Status) : IQuery<List<PaymentDto>>;