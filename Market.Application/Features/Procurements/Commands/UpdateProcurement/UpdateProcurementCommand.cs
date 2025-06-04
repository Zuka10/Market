using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Procurements.Commands.UpdateProcurement;

public record UpdateProcurementCommand(
    long ProcurementId,
    long VendorId,
    long LocationId,
    string ReferenceNo,
    DateTime ProcurementDate,
    decimal TotalAmount,
    string? Notes
) : ICommand<bool>;