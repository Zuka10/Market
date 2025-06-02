using Market.Application.Common.Interfaces;

namespace Market.Application.Features.ProcurementDetails.Commands.DeleteProcurementDetail;

public record DeleteProcurementDetailCommand(long ProcurementDetailId) : ICommand<bool>;