using Market.Application.Common.Interfaces;
using Market.Domain.Enums;

namespace Market.Application.Features.Payments.Commands.UpdatePaymentStatus;

public record UpdatePaymentStatusCommand(
    long PaymentId,
    PaymentStatus Status
) : ICommand<bool>;