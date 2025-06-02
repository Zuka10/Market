using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Payments.Commands.CancelPayment;

public record CancelPaymentCommand(long PaymentId) : ICommand<bool>;