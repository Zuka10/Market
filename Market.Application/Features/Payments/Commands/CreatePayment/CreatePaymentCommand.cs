using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;
using Market.Domain.Enums;

namespace Market.Application.Features.Payments.Commands.CreatePayment;

public record CreatePaymentCommand(
    long OrderId,
    decimal Amount,
    PaymentMethod PaymentMethod,
    DateTime? PaymentDate = null
) : ICommand<PaymentDto>;