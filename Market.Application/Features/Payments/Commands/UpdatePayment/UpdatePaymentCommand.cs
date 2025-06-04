using Market.Application.Common.Interfaces;
using Market.Domain.Enums;

namespace Market.Application.Features.Payments.Commands.UpdatePayment;

public record UpdatePaymentCommand(
    long PaymentId,
    decimal Amount,
    PaymentMethod PaymentMethod,
    DateTime PaymentDate
) : ICommand<bool>;