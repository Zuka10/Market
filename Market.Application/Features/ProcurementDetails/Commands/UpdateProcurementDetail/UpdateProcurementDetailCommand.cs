using Market.Application.Common.Interfaces;

namespace Market.Application.Features.ProcurementDetails.Commands.UpdateProcurementDetail;

public record UpdateProcurementDetailCommand(
    long ProcurementDetailId,
    long ProductId,
    decimal PurchasePrice,
    int Quantity,
    decimal LineTotal
) : ICommand<bool>;