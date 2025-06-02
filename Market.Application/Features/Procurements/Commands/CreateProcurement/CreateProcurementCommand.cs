using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Procurements.Commands.CreateProcurement;

public record CreateProcurementCommand(
    long VendorId,
    long LocationId,
    string ReferenceNo,
    DateTime ProcurementDate,
    decimal TotalAmount,
    string? Notes,
    List<ProcurementDetailDto> ProcurementDetails
) : ICommand<ProcurementDto>;